#!/usr/bin/env python3
"""
CD Webhook Receiver for Machine B (CD Machine).
Listens on port 9000 for deployment triggers from Jenkins (Machine A).
When POST /deploy is called with {"tag": "..."}, it:
  1. Logs the request and extracts the image tag.
  2. Updates IMAGE_TAG in process environment and .env file.
  3. Executes `docker compose pull api` and `docker compose up -d api`.
  4. Returns JSON HTTP 200 status back to Jenkins.

Uses only Python 3 standard library (no pip packages required).
"""

import json
import logging
import os
import re
import subprocess
import sys
from datetime import datetime
from http.server import BaseHTTPRequestHandler, HTTPServer
try:
    from http.server import ThreadingHTTPServer
    SERVER_BASE = ThreadingHTTPServer
except ImportError:
    SERVER_BASE = HTTPServer

# Paths & Settings
DEPLOY_DIR = os.path.dirname(os.path.abspath(__file__))
LOG_FILE = os.path.join(DEPLOY_DIR, "webhook.log")
ENV_FILE = os.path.join(DEPLOY_DIR, ".env")
PORT = int(os.environ.get("WEBHOOK_PORT", 9000))
HOST = os.environ.get("WEBHOOK_HOST", "0.0.0.0")

# Setup logging
logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s [%(levelname)s] %(message)s",
    handlers=[
        logging.StreamHandler(sys.stdout),
        logging.FileHandler(LOG_FILE, encoding="utf-8")
    ]
)
logger = logging.getLogger("CDWebhookReceiver")


def get_compose_command():
    """Detect whether 'docker compose' (v2) or 'docker-compose' (v1) is available."""
    try:
        res = subprocess.run(
            ["docker", "compose", "version"],
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE,
            text=True,
            timeout=5
        )
        if res.returncode == 0:
            return ["docker", "compose"]
    except Exception:
        pass
    return ["docker-compose"]


def update_env_file(tag: str):
    """Update or append IMAGE_TAG in the .env file in the deploy directory."""
    try:
        lines = []
        tag_found = False
        if os.path.isfile(ENV_FILE):
            with open(ENV_FILE, "r", encoding="utf-8") as f:
                for line in f:
                    if line.strip().startswith("IMAGE_TAG="):
                        lines.append(f"IMAGE_TAG={tag}\n")
                        tag_found = True
                    else:
                        lines.append(line)
        if not tag_found:
            lines.append(f"IMAGE_TAG={tag}\n")

        with open(ENV_FILE, "w", encoding="utf-8") as f:
            f.writelines(lines)
        logger.info(f"Updated .env with IMAGE_TAG={tag}")
    except Exception as ex:
        logger.warning(f"Could not update .env file: {ex}")


def execute_deployment(tag: str):
    """
    Run docker compose pull api and docker compose up -d api.
    Returns (success: bool, pull_output: str, up_output: str, error_message: str)
    """
    compose_cmd = get_compose_command()
    cmd_env = os.environ.copy()
    cmd_env["IMAGE_TAG"] = tag

    pull_cmd = compose_cmd + ["pull", "api"]
    up_cmd = compose_cmd + ["up", "-d", "api"]

    logger.info(f"Executing pull command: {' '.join(pull_cmd)} (IMAGE_TAG={tag})")
    pull_proc = subprocess.run(
        pull_cmd,
        cwd=DEPLOY_DIR,
        env=cmd_env,
        stdout=subprocess.PIPE,
        stderr=subprocess.PIPE,
        text=True,
        timeout=300
    )

    pull_output = (pull_proc.stdout + "\n" + pull_proc.stderr).strip()
    logger.info(f"Pull result (return code {pull_proc.returncode}):\n{pull_output}")

    if pull_proc.returncode != 0:
        return False, pull_output, "", f"Failed to pull image tag '{tag}': {pull_proc.stderr}"

    logger.info(f"Executing up command: {' '.join(up_cmd)} (IMAGE_TAG={tag})")
    up_proc = subprocess.run(
        up_cmd,
        cwd=DEPLOY_DIR,
        env=cmd_env,
        stdout=subprocess.PIPE,
        stderr=subprocess.PIPE,
        text=True,
        timeout=180
    )

    up_output = (up_proc.stdout + "\n" + up_proc.stderr).strip()
    logger.info(f"Up result (return code {up_proc.returncode}):\n{up_output}")

    if up_proc.returncode != 0:
        return False, pull_output, up_output, f"Failed to restart container: {up_proc.stderr}"

    return True, pull_output, up_output, ""


class WebhookRequestHandler(BaseHTTPRequestHandler):
    server_version = "CDWebhookReceiver/1.0"

    def _send_json(self, status_code: int, data: dict):
        response_bytes = json.dumps(data, indent=2, ensure_ascii=False).encode("utf-8")
        self.send_response(status_code)
        self.send_header("Content-Type", "application/json; charset=utf-8")
        self.send_header("Content-Length", str(len(response_bytes)))
        self.send_header("Access-Control-Allow-Origin", "*")
        self.end_headers()
        self.wfile.write(response_bytes)

    def do_GET(self):
        client_ip = self.client_address[0]
        logger.info(f"GET {self.path} from {client_ip}")

        if self.path in ("/", "/health", "/status"):
            self._send_json(200, {
                "status": "healthy",
                "service": "Machine B CD Webhook Receiver",
                "current_image_tag": os.environ.get("IMAGE_TAG", "latest"),
                "timestamp": datetime.now().isoformat()
            })
        else:
            self._send_json(404, {
                "status": "error",
                "message": f"Endpoint '{self.path}' not found. Use POST /deploy to trigger deployment."
            })

    def do_POST(self):
        client_ip = self.client_address[0]
        logger.info(f"POST {self.path} from {client_ip}")

        if self.path != "/deploy":
            self._send_json(404, {
                "status": "error",
                "message": f"Endpoint '{self.path}' not found. Expected POST /deploy"
            })
            return

        # Read body
        content_length_header = self.headers.get("Content-Length")
        if not content_length_header:
            self._send_json(400, {
                "status": "error",
                "message": "Missing Content-Length header"
            })
            return

        try:
            content_length = int(content_length_header)
            raw_body = self.rfile.read(content_length).decode("utf-8")
            payload = json.loads(raw_body) if raw_body.strip() else {}
        except Exception as ex:
            logger.error(f"Failed to parse JSON body: {ex}")
            self._send_json(400, {
                "status": "error",
                "message": f"Invalid JSON payload: {str(ex)}"
            })
            return

        # Extract tag
        tag = payload.get("tag") or payload.get("image_tag")
        if not tag:
            logger.warning("Request missing 'tag' field, defaulting to 'latest'")
            tag = "latest"

        # Sanitize tag
        tag = str(tag).strip()
        if not re.match(r"^[a-zA-Z0-9_\-\.]+$", tag):
            self._send_json(400, {
                "status": "error",
                "message": f"Invalid tag format: '{tag}'. Only alphanumeric, dots, hyphens and underscores allowed."
            })
            return

        logger.info(f"--> Received deploy trigger for tag: {tag} from {client_ip}")

        # Update environment and .env file
        os.environ["IMAGE_TAG"] = tag
        update_env_file(tag)

        # Execute deployment
        success, pull_out, up_out, err_msg = execute_deployment(tag)

        if success:
            logger.info(f" Deployment completed successfully for tag: {tag}")
            self._send_json(200, {
                "status": "success",
                "message": f"Deployment for tag '{tag}' completed successfully on Machine B",
                "tag": tag,
                "timestamp": datetime.now().isoformat(),
                "details": {
                    "pull": pull_out,
                    "up": up_out
                }
            })
        else:
            logger.error(f"Deployment failed for tag {tag}: {err_msg}")
            self._send_json(500, {
                "status": "failed",
                "message": f"Deployment failed: {err_msg}",
                "tag": tag,
                "timestamp": datetime.now().isoformat(),
                "details": {
                    "pull": pull_out,
                    "up": up_out
                }
            })

    def log_message(self, format, *args):
        # Delegate standard HTTP server logs to logger
        logger.debug(f"{self.client_address[0]} - {format % args}")


def run_server():
    server_address = (HOST, PORT)
    httpd = SERVER_BASE(server_address, WebhookRequestHandler)
    logger.info(f"==================================================")
    logger.info(f" CD Webhook Receiver running on {HOST}:{PORT}")
    logger.info(f" Deploy directory: {DEPLOY_DIR}")
    logger.info(f" Listening for POST http://<Machine_B_IP>:{PORT}/deploy")
    logger.info(f"==================================================")
    try:
        httpd.serve_forever()
    except KeyboardInterrupt:
        logger.info("Stopping webhook server...")
    finally:
        httpd.server_close()
        logger.info("Server stopped.")


if __name__ == "__main__":
    run_server()
