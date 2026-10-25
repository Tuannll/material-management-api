pipeline {
    agent any

    environment {
        // Cấu hình IP và Port: cập nhật theo IP LAN thực tế giữa Máy A và Máy B
        REGISTRY_HOST    = '192.168.61.1:5000'
        REGISTRY_CRED_ID = 'registry-credentials'
        IMAGE_NAME       = 'material-management-api'
        CD_HOST          = '192.168.61.128'
        CD_PORT          = '9000'
    }

    stages {
        stage('Checkout') {
            steps {
                echo "Pulling source code from repository..."
                checkout scm
            }
        }

        stage('Test') {
            steps {
                echo "Running Unit Tests..."
                script {
                    if (isUnix()) {
                        sh '''
                            if command -v dotnet >/dev/null 2>&1; then
                                dotnet test TH4.sln -c Release --verbosity normal
                            else
                                docker run --rm --volumes-from jenkins -w "${WORKSPACE}" mcr.microsoft.com/dotnet/sdk:10.0 dotnet test TH4.sln -c Release --verbosity normal
                            fi
                        '''
                    } else {
                        bat 'dotnet test TH4.sln -c Release --verbosity normal'
                    }
                }
            }
        }

        stage('Build Image') {
            steps {
                echo "Building Docker Image version v${env.BUILD_NUMBER}..."
                script {
                    def imageTag = "v${env.BUILD_NUMBER}"
                    if (isUnix()) {
                        sh "docker build -t ${REGISTRY_HOST}/${IMAGE_NAME}:${imageTag} -t ${REGISTRY_HOST}/${IMAGE_NAME}:latest ."
                    } else {
                        bat "docker build -t ${REGISTRY_HOST}/${IMAGE_NAME}:${imageTag} -t ${REGISTRY_HOST}/${IMAGE_NAME}:latest ."
                    }
                }
            }
        }

        stage('Push Image') {
            steps {
                echo "Pushing Docker Image to Registry ${REGISTRY_HOST}..."
                withCredentials([usernamePassword(credentialsId: env.REGISTRY_CRED_ID, usernameVariable: 'REG_USER', passwordVariable: 'REG_PASS')]) {
                    script {
                        def imageTag = "v${env.BUILD_NUMBER}"
                        if (isUnix()) {
                            sh """
                                echo "${REG_PASS}" | docker login ${REGISTRY_HOST} -u ${REG_USER} --password-stdin
                                docker push ${REGISTRY_HOST}/${IMAGE_NAME}:${imageTag}
                                docker push ${REGISTRY_HOST}/${IMAGE_NAME}:latest
                            """
                        } else {
                            bat """
                                echo ${REG_PASS} | docker login ${REGISTRY_HOST} -u ${REG_USER} --password-stdin
                                docker push ${REGISTRY_HOST}/${IMAGE_NAME}:${imageTag}
                                docker push ${REGISTRY_HOST}/${IMAGE_NAME}:latest
                            """
                        }
                    }
                }
            }
        }

        stage('Trigger Deploy') {
            steps {
                echo "Triggering CD deployment on Machine B (http://${CD_HOST}:${CD_PORT}/deploy)..."
                script {
                    def imageTag = "v${env.BUILD_NUMBER}"
                    if (isUnix()) {
                        sh """
                            curl -f -s -S -X POST http://${CD_HOST}:${CD_PORT}/deploy \
                                -H "Content-Type: application/json" \
                                -d '{"image": "${REGISTRY_HOST}/${IMAGE_NAME}", "tag": "${imageTag}"}'
                        """
                    } else {
                        bat """
                            curl -f -s -S -X POST http://${CD_HOST}:${CD_PORT}/deploy -H "Content-Type: application/json" -d "{\\"image\\": \\"${REGISTRY_HOST}/${IMAGE_NAME}\\", \\"tag\\": \\"${imageTag}\\"}"
                        """
                    }
                }
            }
        }
    }

    post {
        always {
            script {
                if (isUnix()) {
                    sh "docker logout ${REGISTRY_HOST} || true"
                } else {
                    bat "docker logout ${REGISTRY_HOST} || exit 0"
                }
            }
        }
        success {
            echo "CI/CD Pipeline SUCCEEDED. Tag v${env.BUILD_NUMBER} deployed successfully."
        }
        failure {
            echo "CI/CD Pipeline FAILED. Docker image not pushed and deploy not triggered."
        }
    }
}
