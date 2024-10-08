qaSetting = [
                    "Android_AddressalbeFTP_PATH": "",
                    "Android_Addressalbe_PATH": "",
                    "Android_BuildFTP_Path": "",
                    "Android_SymbolFTP_Path": "",
                    "PullBranch": "QA",
                    "AddressalbeProfile": "QA",
                ]
                
devSetting =    [
                    "Android_AddressalbeFTP_PATH": ",
                    "Android_Addressalbe_PATH": "",
                    "Android_BuildFTP_Path": "",
                    "Android_SymbolFTP_Path": "",
                    "PullBranch": "Dev",
                    "AddressalbeProfile": "Dev",
                ]

productionSetting =    [
                    "Android_AddressalbeFTP_PATH": "",
                    "Android_Addressalbe_PATH": "",
                    "Android_BuildFTP_Path": "",
                    "Android_SymbolFTP_Path": "",
                    "PullBranch": "Production",
                    "AddressalbeProfile": "Production",
                ]

UNITY_PATH = "C:\\Program Files\\Unity\\Hub\\Editor\\2022.3.10f1\\Editor\\Unity.exe"
//UNITY_PATH = "C:\\Program Files\\Unity\\Hub\\Editor\\2022.1.20f1\\Editor\\Unity.exe "
PROJECT_PATH = "C:\\Git\\anotherworld_client\\AnotherWorld_v1"//"C:\\Users\\user\\Desktop\\Jenkins\\TestProject\\testProject"
GIT_PATH = "C:\\Git\\anotherworld_client"
KEYSTORE_NAME = "//"C:/Users/user/Desktop/Jenkins/.jenkins/workspace/AnotherWorldDevelopPipeLine/KeyStore/wemadblock.debug.keystore"
KEYSTORE_PASS = ""
KEYALIAS_NAME = ""
KEYALIAS_PASS = ""
UNITY_BUILD_LOG = "C:\\Users\\user\\Desktop\\Jenkins\\UnityLog\\Android_UnityBuildLog.txt"
UNITY_BUILD_LOG_UPLOAD_LOG = "C:\\Users\\user\\Desktop\\Jenkins\\UnityLog\\Android_UnityBuildLog_UploadLog.txt"
UNITY_ADDRESSABLE_LOG = "C:\\Users\\user\\Desktop\\Jenkins\\UnityLog\\Android_UnityAddressableLog.txt"
UNITY_ADDRESSABLE_LOG_UPLOAD_LOG = "C:\\Users\\user\\Desktop\\Jenkins\\UnityLog\\Android_UnityAddressableLog_UploadLog.txt"
UNITY_BUILD_COPY_AND_UPOLAD_LOG = "C:\\Users\\user\\Desktop\\Jenkins\\UnityLog\\Android_Build_CopyAndUpload.txt"
UNITY_ADDRESSABLE_UPOLAD_LOG = "C:\\Users\\user\\Desktop\\Jenkins\\UnityLog\\Android_ADDRESSABLE_UploadLog.txt"
ADDRESSABLE_PATH = "AddressableData\\Android\\**"
GIT_ADDRESS = 'http://anotherworld.internal.dev1.com:9080/client/anotherworld_client.git'
UNITY_BUILD_PATH = "C:\\Users\\user\\Desktop\\Jenkins\\.jenkins\\workspace\\AnotherWorldDevelopPipeLine\\AnotherWorld_v1\\TempBuildAndroid\\AnotherWorld_Release.apk"
UNITY_BUILD_PATH_ALT = ""
ANDROID_LOG_FTP_PATH = ""
BUILD_FILE_PATH = "C:\\Users\\user\\Desktop\\Jenkins\\BuildResult"
ADDRESSABLE_BACKUP_FOLDER = "C:\\Users\\user\\Desktop\\Jenkins\\UnityAddressableBackUp"

pipeline {
    agent any
    // {
    //     label 'AnotherWorldDevelopBuild'
    // }
    
    parameters {
        choice (name : 'AddressalbeProfile', choices : ['Dev', 'QA', 'Production'], description : 'Adressable 프로필 이름')
        
        booleanParam (name : 'GitPull', defaultValue : true, description : 'Pull 여부')
        string (name : 'PullBranch', defaultValue : '', description : '이름이 없으면 어드레서블 설정, 있으면 따로 이 브랜치로 다운받는다.')
        
        //string (name : 'PullBranch', defaultValue : 'Dev', description : 'Pull 받을 브랜치')
        //listGitBranches branchFilter: 'refs/heads.*/(.*)', defaultValue: 'Dev', name: 'PullBranch', type: 'BRANCH', remoteURL: 'http://anotherworld.internal.dev1.com:9080/client/anotherworld_client.git', credentialsId: 'jack'
        
        booleanParam (name : 'AddressableBuild', defaultValue : true, description : 'Adressable 여부')
        booleanParam (name : 'AddressableNewCreate', defaultValue : true, description : 'Adressable 새로 뽑을지 여부')
        
        hidden (name : 'AddressalbeFTP_Name', defaultValue : 'FTP_User', description : 'Adressable FTP 접속 이름')
        hidden (name : 'AddressalbeFTP_Password', defaultValue : '1234', description : 'Adressable FTP 접속 비번')
        
        booleanParam (name : 'BuildAPK', defaultValue : true, description : 'Apk 빌드 여부')
        
        persistentString (name : 'NhnAppVersion', description : '버전')
        string (name : 'ScriptDefineSymbolStr', defaultValue : 'DEVELOPMENT_BUILD,USE_LOCAL_LOGIN,', description : '스크립트 디파인 심볼들(구분은 ,로)')
        
        booleanParam (name : 'Development', defaultValue : false, description : '개발모드 여부')
        choice (name : 'SymbolsZIP', choices : ['Disabled', 'public', 'Debugging'], description : 'Create symbols.zip')
        hidden (name : 'BuildFTP_Name', defaultValue : 'FTP_User', description : 'Build FTP 접속 이름')
        hidden (name : 'BuildFTP_Password', defaultValue : '1234', description : 'Build FTP 접속 비번')
        
        booleanParam (name : 'AppCenterJudge', defaultValue : false, description : 'AppCenter로 배포할지 여부')
    }
    
    options {
        timestamps()
        timeout(time: 500, unit: 'MINUTES')
    }
    
    environment {
        Build_Result = 'false'
        Addressable_Result = 'false'
        Pull_Result = 'false'
    }
    
    stages {
        
        stage('QA') {
            
            when {
                expression { params.AddressalbeProfile == "QA" }
            }
            
            steps {
                script {
                    Setting = qaSetting
                }
            }
        }
        
        stage('DEV') {
            
            when {
                expression { params.AddressalbeProfile == "Dev" }
            }
            
            steps {
                script {
                    Setting = devSetting
                }
            }
        }

        stage('PRODUCTION') {
            
            when {
                expression { params.AddressalbeProfile == "Production" }
            }
            
            steps {
                script {
                    Setting = productionSetting
                }
            }
        }
        
        stage('Alert') {
            steps {
                slackSend color: "good", message: "Android Jenkins Build Start"
            }
        }
        
        stage('PullProfileBranch') {
            when {
                environment name: 'GitPull', value: 'true'
                expression { params.PullBranch == "" }
            }
            steps {
                catchError(buildResult: 'FAILURE', stageResult: 'FAILURE') {
                    script {
                        try
                        {
                            slackSend color: "good", message: "Android GitPull Start"
                            
                            //checkout([$class: 'GitSCM', 
                            //    branches: [[name: '*/' + Setting.get('PullBranch')]],
                            //    doGenerateSubmoduleConfigurations: false,
                            //    //extensions: [[$class: 'CloneOption', shallow: true, timeout: 1000]],
                            //    extensions: [ cloneOption(noTags: true, timeout: 1000) ],
                            //    submoduleCfg: [], 
                            //    userRemoteConfigs: [[url: GIT_ADDRESS]]])
                            //bat "cd ${PROJECT_PATH} && git reset --hard && git checkout ${Setting.get('PullBranch')} && git pull origin ${Setting.get('PullBranch')}"
                            bat "cd ${GIT_PATH} && git reset --hard && git pull origin ${Setting.get('PullBranch')}"
                            
                            Pull_Result = 'true'
                            slackSend color: "0000FF", message: "Android GitPull Success"
                        }
                        catch(e)
                        {
                            slackSend color: "#FF0000" , message: "Android GitPull Fail"
                            
                            throw e
                        }
                    }
                }
            }
        }
        
        stage('PullBranch') {
            when {
                environment name: 'GitPull', value: 'true'
                expression { params.PullBranch != "" }
            }
            steps {
                catchError(buildResult: 'FAILURE', stageResult: 'FAILURE') {
                    script {
                        try
                        {
                            slackSend color: "good", message: "Android GitPull Start"
                            
                            //checkout([$class: 'GitSCM', 
                            //    branches: [[name: '*/' + Setting.get('PullBranch')]],
                            //    doGenerateSubmoduleConfigurations: false,
                            //    //extensions: [[$class: 'CloneOption', shallow: true, timeout: 1000]],
                            //    extensions: [ cloneOption(noTags: true, timeout: 1000) ],
                            //    submoduleCfg: [], 
                            //    userRemoteConfigs: [[url: GIT_ADDRESS]]])
                            //bat "cd ${PROJECT_PATH} && git reset --hard && git checkout ${Setting.get('PullBranch')} && git pull origin ${Setting.get('PullBranch')}"
                            bat "cd ${GIT_PATH} && git reset --hard && git pull origin ${params.PullBranch}"
                            
                            Pull_Result = 'true'
                            slackSend color: "0000FF", message: "Android GitPull Success"
                        }
                        catch(e)
                        {
                            slackSend color: "#FF0000" , message: "Android GitPull Fail"
                            
                            throw e
                        }
                    }
                }
            }
        }
        
        stage('PullCheck') {
            when {
                environment name: 'GitPull', value: 'true'
                expression { Pull_Result == 'false' }
            }
            steps {
                script {
                    error("Pull fail")
                }
            }
        }
        
        stage('ConfigSetting') {
            when {
                environment name: 'AddressableBuild', value: 'true'
            }
            steps {
                script {
                    bat "\"${UNITY_PATH}\" -accept-apiupdate -batchmode -quit -stackTraceLogType Full -buildTarget Android -projectPath ${PROJECT_PATH} -executeMethod Jenkins.AW_Config_Setting -addressableProfileName ${Setting.get('AddressalbeProfile')}"
                }
            }
        }
        
        stage('Addressable Android') {
            when {
                environment name: 'AddressableBuild', value: 'true'
            }
            
            steps {
                catchError(buildResult: 'FAILURE', stageResult: 'FAILURE') {
                    script {
                        try
                        {
                            slackSend color: "good", message: "Android Addressable Start"
                            
                            bat "\"${UNITY_PATH}\" -accept-apiupdate -batchmode -quit -stackTraceLogType Full -buildTarget Android -logFile ${UNITY_ADDRESSABLE_LOG} -projectPath ${PROJECT_PATH} -executeMethod Jenkins.PerformAndroidAddressable -addressableProfileName ${Setting.get('AddressalbeProfile')} -addressableNew ${params.AddressableNewCreate} -addressablePath ${Setting.get('Android_Addressalbe_PATH')} -addressableBackUPFolder ${ADDRESSABLE_BACKUP_FOLDER}"
                            
                            Addressable_Result = 'true'
                            
                            slackSend color: "0000FF", message: "Android Addressable Success"
                        }
                        catch(e)
                        {
                            Addressable_Result = 'false'
                            
                            slackSend color: "#FF0000", message: "Android Addressable Fail"
                            
                            throw e
                        }
                    }
                }
            }
        }
        
        stage('Addressable Android Log Upload') {
            when {
                environment name: 'AddressableBuild', value: 'true'
            }
            
            steps {
                catchError(buildResult: 'FAILURE', stageResult: 'FAILURE') {
                    script {
                        try
                        {
                            bat "\"${UNITY_PATH}\" -accept-apiupdate -batchmode -nographics -quit -buildTarget Android -stackTraceLogType Full -logFile ${UNITY_ADDRESSABLE_LOG_UPLOAD_LOG} -projectPath ${PROJECT_PATH} -executeMethod Jenkins.AndroidAddressableLogUpload -ftpName ${params.AddressalbeFTP_Name} -ftpPassword ${params.AddressalbeFTP_Password} -androidAddressableLogPath ${UNITY_ADDRESSABLE_LOG} -androidLogFTPPath ${ANDROID_LOG_FTP_PATH}"
                        }
                        catch(e)
                        {
                            throw e
                        }
                    }
                }
            }   
        }
        
        stage('Addressable Android Upload') {
            when {
                environment name: 'AddressableBuild', value: 'true'
                expression { Addressable_Result == "true" }
            }
            
            steps {
                catchError(buildResult: 'FAILURE', stageResult: 'FAILURE') {
                    script {
                        try
                        {
                            slackSend color: "good", message: "Android Addressable Upload Start"
                            
                            bat "\"${UNITY_PATH}\" -accept-apiupdate -batchmode -quit -nograhic -stackTraceLogType Full -buildTarget Android -logFile ${UNITY_ADDRESSABLE_UPOLAD_LOG} -projectPath ${PROJECT_PATH} -executeMethod Jenkins.PerformAndroidAddressableFTPUpload -addressableFtpPath ${Setting.get('Android_AddressalbeFTP_PATH')} -ftpName ${params.AddressalbeFTP_Name} -ftpPassword ${params.AddressalbeFTP_Password} -addressablePath ${Setting.get('Android_Addressalbe_PATH')}"
                            
                            slackSend color: "#0000FF", message: "Android Addressable Upload Success"
                        }
                        catch(e)
                        {
                            slackSend color: "#FF0000", message: "Android Addressable Upload Fail"
                            
                            throw e
                        }
                    }
                }
            }
        }
        
        stage('Build Android') {
            when {
                environment name: 'BuildAPK', value: 'true'
            }
            steps {
                catchError(buildResult: 'FAILURE', stageResult: 'FAILURE') {
                    script {
                        try
                        {
                            slackSend color: "good", message: "Android Build Start"
                            
                            bat "\"${UNITY_PATH}\" -accept-apiupdate -batchmode -quit -stackTraceLogType Full -buildTarget Android -projectPath ${PROJECT_PATH} -executeMethod Jenkins.ScriptDefineSymBolSetting -scriptDefineSymblos ${params.ScriptDefineSymbolStr}"
                            //bat "\"${UNITY_PATH}\" -accept-apiupdate -batchmode -quit -stackTraceLogType Full -buildTarget Android -projectPath ${PROJECT_PATH} -executeMethod Jenkins.AppVersionSetting -addressableProfileName ${Setting.get('AddressalbeProfile')} -nhnAppVersion ${params.NhnAppVersion}"
                            bat "\"${UNITY_PATH}\" -accept-apiupdate -batchmode -quit -stackTraceLogType Full -buildTarget Android -logFile ${UNITY_BUILD_LOG} -projectPath ${PROJECT_PATH} -executeMethod Jenkins.PerformAndroidBuild -keystoreName ${KEYSTORE_NAME} -keystorePass ${KEYSTORE_PASS} -keyaliasName ${KEYALIAS_NAME} -keyaliasPass ${KEYALIAS_PASS} -addressableProfileName ${Setting.get('AddressalbeProfile')} -development ${params.Development} -buildSymbols ${params.SymbolsZIP} -buildPath ${UNITY_BUILD_PATH} -nhnAppVersion ${params.NhnAppVersion}"
                            
                            Build_Result = 'true'
                            
                            slackSend color: "0000FF", message: "Android Build Success"
                        }
                        catch(e)
                        {
                            Build_Result = 'false'
                            
                            slackSend color: "FF0000", message: "Android Build Fail"
                            
                            throw e
                        }
                    }
                }
            }
        }
        
        stage('JenkinsUPLoadGitFile') {
            when {
                environment name: 'BuildAPK', value: 'true'
                expression { Build_Result == "true" }
                expression { params.PullBranch == "" }
            }
            steps {
                catchError(/*buildResult: 'FAILURE',*/ stageResult: 'FAILURE') {
                    script {
                        try
                        {
                            slackSend color: "good", message: "Jenkins File Upload Start"
                            
                            //bat "C:\\Users\\user\\Desktop\\Jenkins\\.jenkins\\workspace\\AnotherWorldDevelopPipeLine\\UPLoad.bat"
                            //bat "cd ${Git_PATH} && git add . && git commit -m 'Jenkins Push' && git pull origin HEAD:${Setting.get('PullBranch')} && git push origin ${Setting.get('PullBranch')}"
                            bat "cd ${GIT_PATH} && git pull origin ${Setting.get('PullBranch')} && git add .\\AnotherWorld_v1\\Assets\\Plugins\\Android\\FirebaseCrashlytics.androidlib\\res\\values\\crashlytics_build_id.xml && git add .\\AnotherWorld_v1\\Assets\\Resources\\Build_Version.txt && git commit -m Jenkins_Push && git push origin HEAD:${Setting.get('PullBranch')}"
                            
                            slackSend color: "0000FF", message: "Jenkins File Upload Success"
                        }
                        catch(e)
                        {
                            slackSend color: "FF0000", message: "Jenkins File Upload Fail"
                            
                            throw e
                        }
                    }
                }
            }
        }
        
        stage('JenkinsUPLoadPullBranch') {
            when {
                environment name: 'BuildAPK', value: 'true'
                expression { Build_Result == "true" }
                expression { params.PullBranch != "" }
            }
            steps {
                catchError(/*buildResult: 'FAILURE',*/ stageResult: 'FAILURE') {
                    script {
                        try
                        {
                            slackSend color: "good", message: "Jenkins File Upload Start"
                            
                            //bat "C:\\Users\\user\\Desktop\\Jenkins\\.jenkins\\workspace\\AnotherWorldDevelopPipeLine\\UPLoad.bat"
                            //bat "cd ${Git_PATH} && git add . && git commit -m 'Jenkins Push' && git pull origin HEAD:${Setting.get('PullBranch')} && git push origin ${Setting.get('PullBranch')}"
                            bat "cd ${GIT_PATH} && git pull origin ${params.PullBranch} && git add .\\AnotherWorld_v1\\Assets\\Plugins\\Android\\FirebaseCrashlytics.androidlib\\res\\values\\crashlytics_build_id.xml && git add .\\AnotherWorld_v1\\Assets\\Resources\\Build_Version.txt && git commit -m Jenkins_Push && git push origin"
                            
                            slackSend color: "0000FF", message: "Jenkins File Upload Success"
                        }
                        catch(e)
                        {
                            slackSend color: "FF0000", message: "Jenkins File Upload Fail"
                            
                            throw e
                        }
                    }
                }
            }
        }
        
        stage('Distribute AppCenter') {
            when {
                environment name: 'AppCenterJudge', value: 'true'
                expression { Build_Result == "true" }
            }
            environment {
                APPCENTER_API_TOKEN = 'c37176f25436a3c75d2ac8b4d79523d3b1f1796d' // credentials('c37176f25436a3c75d2ac8b4d79523d3b1f1796d')
            }
            steps {
                appCenter apiToken: APPCENTER_API_TOKEN,
                        ownerName: 'WeMadBlock',
                        appName: 'AnotherWorld-Dev-Android',
                        pathToApp: UNITY_BUILD_PATH_ALT,
                        distributionGroups: 'Collaborators, Programmers'
            }
        }
        
        stage('Build Android Copy And FTP Upload') {
            when {
                environment name: 'BuildAPK', value: 'true'
                expression { Build_Result == "true" }
            }
            steps {
                catchError(buildResult: 'FAILURE', stageResult: 'FAILURE') {
                    script {
                        try
                        {
                            slackSend color: "good", message: "Android Copy and Upload Start"
                            
                            bat "\"${UNITY_PATH}\" -accept-apiupdate -batchmode -nographics -quit -stackTraceLogType Full -buildTarget Android -logFile ${UNITY_BUILD_COPY_AND_UPOLAD_LOG} -projectPath ${PROJECT_PATH} -executeMethod Jenkins.PerformAndroidBuildCopyAndUpload -buildFtpPath ${Setting.get('Android_BuildFTP_Path')} -ftpName ${params.BuildFTP_Name} -ftpPassword ${params.BuildFTP_Password} -addressableProfileName ${Setting.get('AddressalbeProfile')} -buildSymbols ${params.SymbolsZIP} -symbolFtpPath ${Setting.get('Android_SymbolFTP_Path')} -buildPath ${UNITY_BUILD_PATH} -buildFilePATH ${BUILD_FILE_PATH} -development ${params.Development}"
                            
                            slackSend color: "0000FF", message: "Android Copy and Upload Success"
                        }
                        catch(e)
                        {
                            slackSend color: "FF0000", message: "Android Copy and Upload Fail"
                            
                            throw e
                        }
                    }
                }
            }
        }
        
        stage('Build Android Log Upload') {
            when {
                environment name: 'BuildAPK', value: 'true'
            }
            steps {
                catchError(buildResult: 'FAILURE', stageResult: 'FAILURE') {
                    script {
                        try
                        {
                            bat "\"${UNITY_PATH}\" -accept-apiupdate -batchmode -nographics -quit -buildTarget Android -stackTraceLogType Full -logFile ${UNITY_BUILD_LOG_UPLOAD_LOG} -projectPath ${PROJECT_PATH} -executeMethod Jenkins.AndroidBuildLogUpload -ftpName ${params.AddressalbeFTP_Name} -ftpPassword ${params.AddressalbeFTP_Password} -androidLogFTPPath ${ANDROID_LOG_FTP_PATH}"
                        }
                        catch(e)
                        {
                            throw e
                        }
                    }
                }
            }
        }
        
        stage('EndAlert') {
            steps {
                slackSend color: "good", message: "Android JenkinsBuild End"
            }
        }
    }
    
    // post {
    //     // Clean after build
    //     always {
    //         cleanWs(cleanWhenNotBuilt: false,
    //                 deleteDirs: true,
    //                 disableDeferredWipeout: true,
    //                 notFailBuild: true,
    //                 patterns: [[pattern: '**/AnotherWorldDevelopPipeLine/**', type: 'EXCLUDE']])
    //     }
    // }
    
     // stage('Test') {
        //     steps {
        //         catchError(stageResult: 'FAILURE') {
        //             script {
        //                 try
        //                 {
        //                     //bat "\"${UNITY_PATH}\" -accept-apiupdate -batchmode -nographics -quit -buildTarget Android -stackTraceLogType Full -logFile ${UNITY_ADDRESSABLE_LOG_UPLOAD_LOG} -projectPath ${PROJECT_PATH} -executeMethod Jenkins.AndroidAddressableLogUpload -ftpName ${params.AddressalbeFTP_Name} -ftpPassword ${params.AddressalbeFTP_Password} -androidAddressableLogPath ${UNITY_ADDRESSABLE_LOG} -androidLogFTPPath ${ANDROID_LOG_FTP_PATH}"
                            
        //                     ftpPublisher alwaysPublishFromMaster: false, 
        //                     masterNodeName: '',
        //                     paramPublish: [ parameterName: "" ],
        //                     continueOnError: false, 
        //                     failOnError: false, 
        //                     publishers: 
        //                     [ [
        //                     configName: 'BuildLog', 
        //                     transfers: 
        //                     [
        //                         [
        //                             asciiMode: false, 
        //                             cleanRemote: false, 
        //                             excludes: '', 
        //                             flatten: true, 
        //                             makeEmptyDirs: false, 
        //                             noDefaultExcludes: false, 
        //                             patternSeparator: '[, ]+', 
        //                             remoteDirectory: 'Android', 
        //                             remoteDirectorySDF: false, 
        //                             removePrefix: 'UnityLog', 
        //                             sourceFiles: 'UnityLog/Android_UnityAddressableLog.txt'
        //                         ] 
        //                     ], 
        //                     usePromotionTimestamp: false, 
        //                     useWorkspaceInPromotion: false, 
        //                     verbose: true
        //                     ] ]    
        //                 }
        //                 catch(e)
        //                 {
        //                     throw e
        //                 }
        //             }
        //         }
        //     }   
        // }
}
