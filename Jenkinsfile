def label = "corelibrary_2018-12-12_05"

podTemplate(label: label, containers: [
    containerTemplate(name: 'jnlp', image: "${leancode.ACR()}/jenkins-slave-docker"),
    containerTemplate(name: 'dotnet', image: 'microsoft/dotnet:2.2-sdk',
                      command: 'cat', ttyEnabled: true),
], envVars: [
    envVar(key: 'DOTNET_SKIP_FIRST_TIME_EXPERIENCE', value: '1')
]) {
    nodeSlack(label) {
        def scmVars

        stage('Checkout') {
            scmVars = checkout scm
            leancode.configureMyGet()
        }

        stage('Version') {
            def baseVer = sh(
                script: 'cat CHANGELOG.md | awk \'$1$2 ~ /^##[0-9]\\.[0-9](\\.[0-9])?/ { print $2 }\' | head -n 1',
                returnStdout: true).trim()

            env.GIT_COMMIT = scmVars.GIT_COMMIT
            env.VERSION = BRANCH_NAME ==~ /v\d\.\d(\.\d)?/ ? "${baseVer}.${nextBuildNumber()}" : '0.0.0'
            echo "Building version: ${env.VERSION}"
        }

        container('dotnet') {
            stage('Build') {
                sh 'dotnet restore'
                sh 'dotnet build -c Release --no-restore'
            }

            stage('Test') {
                dir('test') {
                    try {
                        sh 'dotnet msbuild /t:RunTests /p:Configuration=Release /p:LogFileName=$PWD/test-results/tests.trx'
                    } finally {
                        step([$class: 'MSTestPublisher', testResultsFile:'test-results/*.trx', failOnError: true, keepLongStdio: true])
                    }
                }
            }

            stage('Pack') {
                sh 'dotnet pack --no-restore -c Release -o $PWD/packed'
            }

            stage('Publish') {
                when (BRANCH_NAME ==~ /v\d\.\d(\.\d)?/) {
                    withCredentials([string(credentialsId: 'LeanCodeMyGetApiKey', variable: 'MYGET_APIKEY')]) {
                        sh "dotnet nuget push -k '$MYGET_APIKEY' -s 'https://www.myget.org/F/leancode/api/v2/package' 'packed/*.nupkg'"
                    }
                }
            }
        }
    }
}
