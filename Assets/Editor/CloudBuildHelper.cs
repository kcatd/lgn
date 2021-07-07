#if UNITY_EDITOR && UNITY_WEBGL || UNITY_CLOUD_BUILD && UNITY_WEBGL
using System.Collections.Generic;
using UnityEditor.Callbacks;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
#endif
#if UNITY_CLOUD_BUILD
using Constants;
using Amazon.Runtime;
using Amazon.S3.Transfer;
using Amazon;
using Amazon.S3;
using KitsuneAPI;
using UnityEngine.AddressableAssets;
#endif
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using KitsuneAPI.KitsuneUnity;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class CloudBuildHelper : MonoBehaviour
{
#if UNITY_CLOUD_BUILD
	public static void PreExport(UnityEngine.CloudBuild.BuildManifestObject manifest)
	{
		/// Major Version 	1.x
		/// Minor Version 	x.1
		/// Build Number 	x.x.1
		string build = manifest.GetValue("buildNumber", null);
		PlayerSettings.bundleVersion = string.Format("{0}.{1}", PlayerSettings.bundleVersion, build);
		PlayerSettings.iOS.buildNumber = build;
   
		int buildNo = 1;
		int.TryParse(build, out buildNo);

#if UNITY_ANDROID
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
#endif
		PlayerSettings.Android.bundleVersionCode = buildNo;
    }
#endif
	
#if UNITY_EDITOR && UNITY_WEBGL || UNITY_CLOUD_BUILD && UNITY_WEBGL
    //The name of the WebGLTemplate. Location in project should be Assets/WebGLTemplates/<YOUR TEMPLATE NAME>
    const string WEB_GL_TEMPLATE = "Main";
 
    [PostProcessBuild(1)]
    public static void ChangeWebGLTemplate(BuildTarget buildTarget, string pathToBuiltProject)
    {
        //create template path
        var templatePath = Paths.Combine(Application.dataPath, "WebGLTemplate", WEB_GL_TEMPLATE);
 
        //Clear the TemplateData folder, built by Unity.
        FileUtilExtended.CreateOrCleanDirectory(Paths.Combine(pathToBuiltProject, "TemplateData"));
 
        //Copy contents from WebGLTemplate. Ignore all .meta files
        FileUtilExtended.CopyDirectoryFiltered(templatePath, pathToBuiltProject, true, @".*/\.+|\.meta$", true);
 
        //Replace contents of index.html
        string cacheBuster = "?v=" + Application.version + "_" + Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
        FixIndexHtml(pathToBuiltProject);
        FixGameJs(pathToBuiltProject);
        FixLoader(pathToBuiltProject);
        WriteVersionFile(pathToBuiltProject);
    }
 
    // Replaces {{{ ... }}} defines in index.html
    static void FixIndexHtml(string pathToBuiltProject)
    {
        string baseName = Path.GetFileName(pathToBuiltProject);

        // Build Keywords map
        Dictionary<string, string> replaceKeywordsMap = new Dictionary<string, string>();
        
        replaceKeywordsMap.Add("{{{ WIDTH }}}", PlayerSettings.defaultWebScreenWidth.ToString());
        replaceKeywordsMap.Add("{{{ HEIGHT }}}",  PlayerSettings.defaultWebScreenHeight.ToString());
        replaceKeywordsMap.Add("{{{ PRODUCT_NAME }}}", Application.productName);
        replaceKeywordsMap.Add("{{{ PRODUCT_VERSION }}}", Application.version);
 
        string indexFilePath = Paths.Combine(pathToBuiltProject, "index.html");
        Func<string, KeyValuePair<string, string>, string> replaceFunction = (current, replace) => string.IsNullOrEmpty(replace.Value)? current : current.Replace(replace.Key, replace.Value);
        if (File.Exists(indexFilePath))
        {
            File.WriteAllText(indexFilePath, replaceKeywordsMap.Aggregate<KeyValuePair<string, string>, string>(File.ReadAllText(indexFilePath), replaceFunction));
        }
    }
    static void FixGameJs(string pathToBuiltProject)
    {
        string baseName = Path.GetFileName(pathToBuiltProject);
 
        // Build Keywords map
        Dictionary<string, string> replaceKeywordsMap = new Dictionary<string, string>();

        string suffix = "";

        if (PlayerSettings.WebGL.compressionFormat == WebGLCompressionFormat.Gzip)
        {
            suffix = ".gz";
        }
        else if (PlayerSettings.WebGL.compressionFormat == WebGLCompressionFormat.Brotli)
        {
            suffix = ".br";
        }
        
        replaceKeywordsMap.Add("{{{ LOADER_FILENAME }}}", baseName + ".loader.js");
        replaceKeywordsMap.Add("{{{ DATA_FILENAME }}}", baseName + ".data" + suffix);
        replaceKeywordsMap.Add("{{{ CODE_FILENAME }}}", baseName + ".wasm" + suffix);
        replaceKeywordsMap.Add("{{{ FRAMEWORK_FILENAME }}}", baseName + ".framework.js" + suffix);
        replaceKeywordsMap.Add("{{{ BACKGROUND_FILENAME }}}", baseName + ".jpg");
 
        //App info
        replaceKeywordsMap.Add("{{{ COMPANY_NAME }}}", Application.companyName);
        replaceKeywordsMap.Add("{{{ PRODUCT_NAME }}}", Application.productName);
        replaceKeywordsMap.Add("{{{ PRODUCT_VERSION }}}", Application.version);
        
 
        string indexFilePath = Paths.Combine(pathToBuiltProject, "game.js");
        Func<string, KeyValuePair<string, string>, string> replaceFunction = (current, replace) => string.IsNullOrEmpty(replace.Value)? current : current.Replace(replace.Key, replace.Value);
        if (File.Exists(indexFilePath))
        {
            File.WriteAllText(indexFilePath, replaceKeywordsMap.Aggregate<KeyValuePair<string, string>, string>(File.ReadAllText(indexFilePath), replaceFunction));
        }
    }
    
    public static void WriteVersionFile(string pathToBuiltProject)
    {
        Dictionary<string, string> replaceKeywordsMap = new Dictionary<string, string>();
        replaceKeywordsMap.Add("{{{ PRODUCT_VERSION }}}", Application.version);
        
        string indexFilePath = Paths.Combine(pathToBuiltProject, "version.json");
        Func<string, KeyValuePair<string, string>, string> replaceFunction = (current, replace) => string.IsNullOrEmpty(replace.Value)? current : current.Replace(replace.Key, replace.Value);
        if (File.Exists(indexFilePath))
        {
            File.WriteAllText(indexFilePath, replaceKeywordsMap.Aggregate<KeyValuePair<string, string>, string>(File.ReadAllText(indexFilePath), replaceFunction));
        }
    }

    static String renameFile(String path, String baseName, String suffix)
    {
        String newName = "lgnpoker_v" + Application.version + "_" + Path.GetRandomFileName() + suffix;
        File.Move(path + "/" + baseName + suffix, path + "/" + newName);
        return newName;
    }

    static void FixLoader(string pathToBuiltProject)
    {
        string baseName = Path.GetFileName(pathToBuiltProject);
        
        // Build Keywords map
        Dictionary<string, string> replaceKeywordsMap = new Dictionary<string, string>();

        if (Debug.isDebugBuild)
        {
            replaceKeywordsMap.Add("['Safari', 'Safari'],", "['Safari', 'Safari'],\n      ['ZoomApps', 'ZoomApps'],\n     ['Zoom', 'Zoom'],\n     ['Mozilla', 'Mozilla'],");
        }
        else
        {
            replaceKeywordsMap.Add("[\"Safari\",\"Safari\"]", "[\"Safari\",\"Safari\"],[\"ZoomApps\",\"ZoomApps\"][\"Zoom\",\"Zoom\"],[\"Mozilla\",\"Mozilla\"]");
        }

        string indexFilePath = Paths.Combine(pathToBuiltProject, "Build/" + baseName + ".loader.js");
        Func<string, KeyValuePair<string, string>, string> replaceFunction = (current, replace) => string.IsNullOrEmpty(replace.Value)? current : current.Replace(replace.Key, replace.Value);
        if (File.Exists(indexFilePath))
        {
            File.WriteAllText(indexFilePath, replaceKeywordsMap.Aggregate<KeyValuePair<string, string>, string>(File.ReadAllText(indexFilePath), replaceFunction));
        }
    }
    
    
    static Dictionary<string, string> ComputeFileHashes(string directory)
    {
        var hashes = new Dictionary<string, string>();
        if (Directory.Exists(directory))
        {
            // Create a DirectoryInfo object representing the specified directory.
            var dir = new DirectoryInfo(directory);
            // Get the FileInfo objects for every file in the directory.
            FileInfo[] files = dir.GetFiles();
            // Initialize a SHA256 hash object.
            using (SHA256 mySHA256 = SHA256.Create())
            {
                // Compute and print the hash values for each file in directory.
                foreach (FileInfo fInfo in files)
                {
                    try {
                        // Create a fileStream for the file.
                        FileStream fileStream = fInfo.Open(FileMode.Open);
                        // Be sure it's positioned to the beginning of the stream.
                        fileStream.Position = 0;
                        // Compute the hash of the fileStream.
                        byte[] hashValue = mySHA256.ComputeHash(fileStream);
                        // Write the name and hash value of the file to the console.
                        string hashString = Convert.ToBase64String(hashValue);
                        Console.Write($"{fInfo.Name}: " + hashString);
                        hashes[fInfo.Name] = hashString;
                        //PrintByteArray(hashValue);
                        // Close the file.
                        fileStream.Close();
                    }
                    catch (IOException e) {
                        Console.WriteLine($"I/O Exception: {e.Message}");
                    }
                    catch (UnauthorizedAccessException e) {
                        Console.WriteLine($"Access Exception: {e.Message}");
                    }
                }
            }
        }
        else
        {
            Console.WriteLine("The directory specified could not be found.");
        }
        return hashes;
    }
    private class FileUtilExtended
    {
        internal static void CreateOrCleanDirectory(string dir)
        {
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }
            Directory.CreateDirectory(dir);
        }
 
        //Fix forward slashes on other platforms than windows
        internal static string FixForwardSlashes(string unityPath)
        {
            return ((Application.platform != RuntimePlatform.WindowsEditor) ? unityPath : unityPath.Replace("/", @"\"));
        }
        
        //Copies the contents of one directory to another.
        public static void CopyDirectoryFiltered(string source, string target, bool overwrite, string regExExcludeFilter, bool recursive)
        {
            RegexMatcher excluder = new RegexMatcher()
            {
                exclude = null
            };
            try
            {
                if (regExExcludeFilter != null)
                {
                    excluder.exclude = new Regex(regExExcludeFilter);
                }
            }
            catch (ArgumentException)
            {
               UnityEngine.Debug.Log("CopyDirectoryRecursive: Pattern '" + regExExcludeFilter + "' is not a correct Regular Expression. Not excluding any files.");
                return;
            }
            CopyDirectoryFiltered(source, target, overwrite, excluder.CheckInclude, recursive);
        }
       
        internal static void CopyDirectoryFiltered(string sourceDir, string targetDir, bool overwrite, Func<string, bool> filtercallback, bool recursive)
        {
            // Create directory if needed
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
                overwrite = false;
            }
 
            // Iterate all files, files that match filter are copied.
            foreach (string filepath in Directory.GetFiles(sourceDir))
            {
                if (filtercallback(filepath))
                {
                    string fileName = Path.GetFileName(filepath);
                    string to = Path.Combine(targetDir, fileName);
 
                 
                    File.Copy(FixForwardSlashes(filepath),FixForwardSlashes(to), overwrite);
                }
            }
 
            // Go into sub directories
            if (recursive)
            {
                foreach (string subdirectorypath in Directory.GetDirectories(sourceDir))
                {
                    if (filtercallback(subdirectorypath))
                    {
                        string directoryName = Path.GetFileName(subdirectorypath);
                       CopyDirectoryFiltered(Path.Combine(sourceDir, directoryName), Path.Combine(targetDir, directoryName), overwrite, filtercallback, recursive);
                    }
                }
            }
        }
 
        internal struct RegexMatcher
        {
            public Regex exclude;
            public bool CheckInclude(string s)
            {
                return exclude == null || !exclude.IsMatch(s);
            }
        }
 
    }
 
    private class Paths
    {
        //Combine multiple paths using Path.Combine
        public static string Combine(params string[] components)
        {
            if (components.Length < 1)
            {
                throw new ArgumentException("At least one component must be provided!");
            }
            string str = components[0];
            for (int i = 1; i < components.Length; i++)
            {
                str = Path.Combine(str, components[i]);
            }
            return str;
        }
    }
#endif
    
#if UNITY_CLOUD_BUILD
	private static readonly RegionEndpoint _bucketRegion = RegionEndpoint.USWest2;
	private static IAmazonS3 _s3;
    public static void PostExport(string pathToBuiltProject)
    {
        Debug.Log("pathToBuiltProject " + pathToBuiltProject);
        
        UploadAddressables(pathToBuiltProject);
        
#if UNITY_WEBGL
        UploadWebGLToS3(pathToBuiltProject);
#endif
    }

    public static void UploadAddressables(string pathToBuiltProject)
    {
        string buildTarget = EditorUserBuildSettings.activeBuildTarget.ToString();
        string s3EnvBucket =
            AWSSettings.GetBucketFromServerSettings(KitsuneManager.GameSettings.ServerSettings.Name);
        string toPath = s3EnvBucket + "/" + AWSSettings.S3_GAME_DIR + "/" + buildTarget + 
            "/" + Application.version;

        Debug.Log("AWS: Uploading files to " + toPath);
        
        string buildPathDir = pathToBuiltProject.Substring(0, pathToBuiltProject.LastIndexOf("/"));
        string buildPath = buildPathDir.Substring(0, buildPathDir.LastIndexOf("/"));
        string serverDataPath = Path.Combine(buildPath, "ServerData/" + buildTarget);
        
        Debug.Log("AWS: Uploading files from " + serverDataPath);

        string[] files = Directory.GetFiles(serverDataPath);
        foreach (var file in files)
        {
            UploadToS3(file, toPath);
        }
    }
    
	public static void UploadToS3(string fromPath, string toPath, bool isDirectory = false)
	{
		var awsAccessKey = Environment.GetEnvironmentVariable("aws_access_key");
		var awsSecretKey = Environment.GetEnvironmentVariable("aws_secret_key");

		if (string.IsNullOrEmpty(awsAccessKey) || string.IsNullOrEmpty(awsSecretKey))
			return;
		
		AWSCredentials credentials = new BasicAWSCredentials(awsAccessKey, awsSecretKey);
		
		if (_s3 == null)
		{
			_s3 = new AmazonS3Client(credentials, _bucketRegion);
		}
		
		try
		{
			var fileTransfer = new TransferUtility(_s3);
			if (isDirectory)
			{
				fileTransfer.UploadDirectory(fromPath, toPath);
			}
			else
			{
				fileTransfer.Upload(fromPath, toPath);
			}
		}
		catch (AmazonS3Exception e)
		{
			Debug.LogErrorFormat("Error encountered on server. Message:'{0}' when writing an object", e.Message);
		}
		catch (Exception e)
		{
			Debug.LogErrorFormat("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
		}
	}
	
#if UNITY_WEBGL
    public static void UploadWebGLToS3(string pathToBuiltProject)
    {
        if (KitsuneManager.GameSettings.ServerSettings.Name == EKitsuneEnvironment.Production)
            return;
               
        string s3EnvBucket = AWSSettings.GetBucketFromServerSettings(KitsuneManager.GameSettings.ServerSettings.Name);
        string envPath = s3EnvBucket + "/" + AWSSettings.S3_GAME_DIR + "/" + AWSSettings.S3_WEBGL_BUILDS_BUCKET;
        string[] files = Directory.GetFiles(pathToBuiltProject);
        string[] directories = Directory.GetDirectories(pathToBuiltProject);

        // for (int i = 0; i < files.Length; ++i)
        // {
        //     Debug.Log("$$$ UploadToS3... file=" + files[i]);
        // }
        //
        // for (int i = 0; i < directories.Length; ++i)
        // {
        //     Debug.Log("$$$ UploadToS3... dir=" + directories[i]);
        // }
        
        string buildTarget = EditorUserBuildSettings.activeBuildTarget.ToString();
        string aaStreamingAssetsFromPath = "Library/com.unity.addressables/aa/" + buildTarget;
        string aaStreamingAssetsToPath = envPath + "/StreamingAssets/aa";
        string[] aaFiles = Directory.GetFiles(aaStreamingAssetsFromPath);
        string[] aaDirectories = Directory.GetDirectories(aaStreamingAssetsFromPath);

        Debug.Log("AWS: UploadToS3... .AA Streaming Assets Path=" + aaStreamingAssetsFromPath);

        foreach (var file in aaFiles)
        {
            UploadToS3(file, aaStreamingAssetsToPath);
        }

        string dirPath = "";
        foreach (var dir in aaDirectories)
        {
            string parentDir = dir.Substring(dir.LastIndexOf("/"));
            dirPath = aaStreamingAssetsToPath + parentDir;
            UploadToS3(dir, dirPath, true);
        }
        
        foreach (var file in files)
        {
            UploadToS3(file, envPath);
        }

        foreach (var dir in directories)
        {
            string parentDir = dir.Substring(dir.LastIndexOf("/"));
            dirPath = envPath + parentDir;
            UploadToS3(dir, dirPath, true);
        }
    }
#endif
#endif
}