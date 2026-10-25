#!/usr/bin/env bash
# ==============================================================================
# Setup script for CD Machine (Machine B)
# Sets up environment, initializes SQL Server container, and runs webhook receiver.
# ==============================================================================

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "${SCRIPT_DIR}"

echo "=========================================================="
echo "    CD MACHINE (MACHINE B) - AUTOMATED SETUP & RUNNER     "
echo "=========================================================="

# 1. Check Python
if command -v python3 >/dev/null 2>&1; then
    PYTHON_BIN="python3"
elif command -v python >/dev/null 2>&1; then
    PYTHON_BIN="python"
else
    echo "[ERROR] Python 3 is required but not installed."
    exit 1
fi
echo "[OK] Found Python: $($PYTHON_BIN --version)"

# 2. Check Docker
if ! command -v docker >/dev/null 2>&1; then
    echo "[ERROR] Docker is not installed or not in PATH."
    exit 1
fi

if ! docker info >/dev/null 2>&1; then
    echo "[ERROR] Docker daemon is not running. Please start Docker service."
    exit 1
fi
echo "[OK] Docker daemon is running."

# 3. Detect Docker Compose
if docker compose version >/dev/null 2>&1; then
    COMPOSE_CMD="docker compose"
elif command -v docker-compose >/dev/null 2>&1; then
    COMPOSE_CMD="docker-compose"
else
    echo "[ERROR] Neither 'docker compose' nor 'docker-compose' found."
    exit 1
fi
echo "[OK] Docker Compose detected: $COMPOSE_CMD"

# 4. Prepare .env file
if [ ! -f ".env" ]; then
    if [ -f ".env.example" ]; then
        echo "[INFO] .env not found. Creating from .env.example..."
        cp .env.example .env
        echo "[OK] Created .env file. Customize it as needed."
    elif [ -f "env.example" ]; then
        cp env.example .env
        echo "[OK] Created .env file from env.example."
    fi
else
    echo "[OK] Found existing .env file."
fi

# 5. Start infrastructure (SQL Server database)
echo "[INFO] Starting SQL Server database container..."
$COMPOSE_CMD up -d sqlserver

echo "[INFO] Waiting for SQL Server container to initialize..."
sleep 5

# 6. Display IP and Webhook instructions
# Lấy IP mạng chính kết nối ra ngoài (loại bỏ IP card ảo docker0/veth 172.x)
PRIMARY_IP=$(ip route get 1.1.1.1 2>/dev/null | awk '{print $7}')
if [ -z "$PRIMARY_IP" ]; then
    PRIMARY_IP=$(hostname -I 2>/dev/null | tr ' ' '\n' | grep -v '^172\.' | grep -v '^127\.' | head -n1)
fi
PRIMARY_IP=${PRIMARY_IP:-localhost}

echo "=========================================================="
echo " [CD Machine Ready]"
echo " Local Webhook URL : http://localhost:9000/deploy"
echo " Network Webhook URL: http://${PRIMARY_IP}:9000/deploy"
echo " Healthcheck URL   : http://${PRIMARY_IP}:9000/health"
echo " Web API Port       : 8080"
echo ""
echo " Sample trigger command from Jenkins (Machine A):"
echo "   curl -X POST http://${PRIMARY_IP}:9000/deploy \\"
echo "        -H \"Content-Type: application/json\" \\"
echo "        -d '{\"tag\": \"<build_number_or_sha>\"}'"
echo "=========================================================="

# 7. Start Webhook Receiver
echo "[INFO] Starting Webhook Receiver on port 9000..."
exec "$PYTHON_BIN" "${SCRIPT_DIR}/webhook_receiver.py"
