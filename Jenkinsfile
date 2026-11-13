
pipeline {

 
    options {
        timeout(time: 30, unit: 'MINUTES')             
        buildDiscarder(logRotator(numToKeepStr: '10')) 
        disableConcurrentBuilds()                       
        timestamps()                                    
    }

   

    triggers {
        pollSCM('H/5 * * * *')
    }

    
    parameters {
        booleanParam(name: 'PUSH_IMAGE', defaultValue: false,
                     description: 'Push the image to Docker Hub (needs the dockerhub-creds credential)')
    }

    
    environment {
        DOCKERHUB_USER = 'your-dockerhub-username'      // <-- change this
        IMAGE_NAME     = "${DOCKERHUB_USER}/hospital-api"
        IMAGE_TAG      = "${BUILD_NUMBER}"              // BUILD_NUMBER is built into Jenkins
        APP_PROJECT    = 'src/HospitalApi/HospitalApi.csproj'
        TEST_PROJECT   = 'tests/HospitalApi.Tests/HospitalApi.Tests.csproj'
        DOTNET_CLI_TELEMETRY_OPTOUT = '1'
    }

    stages {

      
        stage('Build') {
            agent {
                docker {
                    image 'mcr.microsoft.com/dotnet/sdk:8.0'
                    args  '-v nuget-cache:/nuget -e NUGET_PACKAGES=/nuget -e DOTNET_CLI_HOME=/tmp'
                }
            }
            steps {
                sh 'dotnet restore $APP_PROJECT'
                sh 'dotnet publish $APP_PROJECT -c Release -o publish --no-restore'
                stash name: 'app', includes: 'publish/**'
                archiveArtifacts artifacts: 'publish/**', fingerprint: true
            }
        }

       
        stage('Test and Scan') {
            failFast true      
            parallel {

                stage('Unit Tests') {
                    agent {
                        docker {
                            image 'mcr.microsoft.com/dotnet/sdk:8.0'
                            args  '-v nuget-cache:/nuget -e NUGET_PACKAGES=/nuget -e DOTNET_CLI_HOME=/tmp'
                        }
                    }
                    steps {
                        sh 'dotnet test $TEST_PROJECT --logger "junit;LogFilePath=TestResults/results.xml"'
                    }
                    post {
                       
                        always {
                            junit 'tests/HospitalApi.Tests/TestResults/results.xml'
                        }
                    }
                }

            
                stage('Dependency Scan') {
                    agent {
                        docker {
                            image 'mcr.microsoft.com/dotnet/sdk:8.0'
                            args  '-v nuget-cache:/nuget -e NUGET_PACKAGES=/nuget -e DOTNET_CLI_HOME=/tmp'
                        }
                    }
                    steps {
                        sh 'dotnet restore $TEST_PROJECT'
                        sh 'dotnet list $TEST_PROJECT package --vulnerable --include-transitive'
                    }
                }
            }
        }

       
        stage('Docker Build') {
            agent any
            when { expression { isMain() } }
            steps {
                unstash 'app'
                sh 'docker build -t $IMAGE_NAME:$IMAGE_TAG -t $IMAGE_NAME:latest .'
            }
        }

      
        stage('Push Image') {
            agent any
            when {
                allOf {
                    expression { isMain() }
                    expression { params.PUSH_IMAGE }
                }
            }
            steps {
                withCredentials([usernamePassword(credentialsId: 'dockerhub-creds',
                                                  usernameVariable: 'DOCKER_USER',
                                                  passwordVariable: 'DOCKER_PASS')]) {
                    // single quotes: the secret is read by the shell, never put in the Groovy string
                    sh '''
                        echo "$DOCKER_PASS" | docker login -u "$DOCKER_USER" --password-stdin
                        docker push $IMAGE_NAME:$IMAGE_TAG
                        docker push $IMAGE_NAME:latest
                        docker logout
                    '''
                }
            }
        }

      
        stage('Deploy Dev') {
            agent any
            when { expression { isMain() } }
            steps {
                sh '''
                    docker rm -f hospital-api-dev || true
                    docker run -d --name hospital-api-dev -p 8081:8080 \
                        -e APP_ENV=development $IMAGE_NAME:$IMAGE_TAG
                '''
            }
        }

      
        stage('Approve Production') {
            agent none
            when { expression { isMain() } }
            options { timeout(time: 15, unit: 'MINUTES') }
            steps {
                input message: 'Deploy to production?', ok: 'Deploy'
            }
        }

       
        stage('Deploy Prod') {
            agent any
            when { expression { isMain() } }
            steps {
                sh '''
                    docker rm -f hospital-api-prod || true
                    docker run -d --name hospital-api-prod -p 8082:8080 \
                        -e APP_ENV=production $IMAGE_NAME:$IMAGE_TAG
                '''
            }
        }
    }

 
    post {
        always  { echo "Build #${BUILD_NUMBER} finished with status: ${currentBuild.currentResult}" }
        success { echo 'Pipeline succeeded.' }
        failure { echo 'Pipeline failed - check the stage that is red.' }
    }
}


def isMain() {
    return env.BRANCH_NAME == 'main' || env.GIT_BRANCH == 'origin/main'
}
