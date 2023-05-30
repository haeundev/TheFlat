using System;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace Microsoft.Acoustics.Editor
{

    public class LegalTextWindow : EditorWindow
    {
        public string LicenseText { get; set; }
        private Vector2 m_scrollPosition;

        public void OnGUI()
        {
            m_scrollPosition = EditorGUILayout.BeginScrollView(m_scrollPosition, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
            GUILayout.TextArea(LicenseText, GUILayout.ExpandWidth(true));
            EditorGUILayout.EndScrollView();
            GUILayout.Space(10);
        }
    }

    public class AcousticsAbout : EditorWindow
    {
        private Vector2 m_scrollPosition;
        private const string m_version = "2022.1.376"; // updated during packaging
        private const string m_projectName = "Project Acoustics version " + m_version;
        private const string m_copyRightLabel = "Copyright Microsoft Corporation";
        private const string m_privacyLabel = "Privacy Terms";
        private const string m_privacyURL = "http://aka.ms/privacy";
        private const string m_servicesAgreementLabel = "Microsoft Software License Terms";
        private const string m_microsoftAgreementFile = "MicrosoftSoftwareUseTerms.txt";
        private const string m_thirdPartyLicenseLabel = "Third Party Notices";
        private const string m_thirdPartyLicenseFile = "THIRDPARTYNOTICES.txt";
        const float LinkButtonWidth = 250;

        public void OnGUI()
        {
            Vector2 windowSize = GUI.skin.label.CalcSize(new GUIContent(m_projectName));
            float windowWidth = 2 * windowSize.x;
            float windowHeight = 11 * windowSize.y;
            this.minSize = new Vector2(windowWidth, windowHeight);
            this.maxSize = this.minSize;
            GUILayout.Space(20);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(m_projectName, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(m_copyRightLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUIContent privacyButtonContent = new GUIContent(m_privacyLabel, m_privacyURL);
            if (GUILayout.Button(privacyButtonContent, GUILayout.Width(LinkButtonWidth)))
            {
                Application.OpenURL(m_privacyURL);
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(m_servicesAgreementLabel, GUILayout.Width(LinkButtonWidth)))
            {
                ShowLicense(m_microsoftAgreementFile, m_servicesAgreementLabel, windowSize);
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(m_thirdPartyLicenseLabel, GUILayout.Width(LinkButtonWidth)))
            {
                ShowLicense(m_thirdPartyLicenseFile, m_thirdPartyLicenseLabel, windowSize);
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);
        }

        void ShowLicense(string filename, string title, Vector2 windowSize)
        {
            LegalTextWindow textWindow = ScriptableObject.CreateInstance<LegalTextWindow>();
            textWindow.LicenseText = LoadLicenseText(filename);
            textWindow.titleContent = new GUIContent(title);
            textWindow.minSize = new Vector2(4 * windowSize.x, 30 * windowSize.y);
            textWindow.maxSize = textWindow.minSize;
            textWindow.ShowUtility();
        }

        string LoadLicenseText(string filename)
        {
            MonoScript thisScript = MonoScript.FromScriptableObject(this);
            string pathToThisScript = Path.GetDirectoryName(AssetDatabase.GetAssetPath(thisScript));
            string unityRootPath = Path.GetDirectoryName(Application.dataPath);
            string licenseFilePath = Path.Combine(unityRootPath, pathToThisScript, filename);
            licenseFilePath = Path.GetFullPath(licenseFilePath); // Normalize the path

            string text = $"License file {licenseFilePath} not found! Please re-install plugin.";

            if (File.Exists(licenseFilePath))
            {
                text = File.ReadAllText(licenseFilePath);
            }

            return text;
        }
    }
}