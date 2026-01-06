using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

/// <summary>
/// Centralized privacy policy and legal documentation system
/// Handles GDPR, CCPA compliance and consent management
/// </summary>
public class PrivacyPolicyManager : Node
{
    public static PrivacyPolicyManager Instance { get; private set; }

    // Privacy policy content
    private string _privacyPolicyPath = "res://legal/PrivacyPolicy.md";
    private string _termsOfServicePath = "res://legal/TermsOfService.md";
    private string _cookiePolicyPath = "res://legal/CookiePolicy.md";
    
    // Consent tracking
    private ConsentData _consentData;
    private bool _hasShownPrivacyNotice = false;
    
    [Signal]
    public delegate void PrivacyConsentChangedEventHandler(bool consented, ConsentData consent);
    
    [Signal]
    public delegate void PrivacyPolicyViewedEventHandler();

    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        
        Instance = this;
        InitializePrivacySystem();
    }

    /// <summary>
    /// Initialize privacy policy system
    /// </summary>
    private void InitializePrivacySystem()
    {
        LoadConsentData();
        CreateLegalDocuments();
        
        GD.Print("Privacy policy system initialized");
    }

    /// <summary>
    /// Create comprehensive privacy policy
    /// </summary>
    private void CreateLegalDocuments()
    {
        CreatePrivacyPolicy();
        CreateTermsOfService();
        CreateCookiePolicy();
        CreateDataRetentionPolicy();
    }

    /// <summary>
    /// Create privacy policy covering all data collection practices
    /// </summary>
    private void CreatePrivacyPolicy()
    {
        string dir = Path.GetDirectoryName(_privacyPolicyPath);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        
        try
        {
            using (var writer = new StreamWriter(_privacyPolicyPath))
            {
                writer.WriteLine("# Privacy Policy");
                writer.WriteLine();
                writer.WriteLine("**Last Updated: January 6, 2025**");
                writer.WriteLine();
                
                writer.WriteLine("## 1. Introduction");
                writer.WriteLine();
                writer.WriteLine("Miff Games (\"we,\" \"our,\" or \"us\") operates the Angry Animals mobile game (the \"Service\"). This Privacy Policy explains how we collect, use, disclose, and safeguard your information when you use our Service.");
                writer.WriteLine();
                
                writer.WriteLine("## 2. Information We Collect");
                writer.WriteLine();
                writer.WriteLine("### 2.1 Information You Provide");
                writer.WriteLine("- **Account Information**: Username, email address (if you choose to create an account)");
                writer.WriteLine("- **Game Data**: Level progress, scores, settings, and preferences");
                writer.WriteLine("- **User Content**: Photos you upload for face customization");
                writer.WriteLine("- **Communications**: Messages you send to our support team");
                writer.WriteLine();
                
                writer.WriteLine("### 2.2 Automatically Collected Information");
                writer.WriteLine("- **Device Information**: Device model, operating system, unique device identifiers");
                writer.WriteLine("- **Game Analytics**: Level completion rates, session duration, feature usage");
                writer.WriteLine("- **Performance Data**: App crashes, error logs, performance metrics");
                writer.WriteLine("- **Advertising Data**: Ad interactions, impressions, and click-through rates");
                writer.WriteLine();
                
                writer.WriteLine("### 2.3 Third-Party Services");
                writer.WriteLine("We may collect information from third-party services:");
                writer.WriteLine("- **AdMob**: Advertising analytics and ad performance data");
                writer.WriteLine("- **Game Analytics**: Crash reporting and performance monitoring");
                writer.WriteLine("- **App Store**: Download and update information");
                writer.WriteLine();
                
                writer.WriteLine("## 3. How We Use Your Information");
                writer.WriteLine();
                writer.WriteLine("We use the collected information for:");
                writer.WriteLine("- **Service Provision**: Providing and maintaining the game");
                writer.WriteLine("- **Personalization**: Customizing game experience based on preferences");
                writer.WriteLine("- **Analytics**: Understanding user behavior to improve the game");
                writer.WriteLine("- **Advertising**: Displaying relevant ads through AdMob");
                writer.WriteLine("- **Security**: Detecting and preventing fraud and abuse");
                writer.WriteLine("- **Communication**: Responding to support requests");
                writer.WriteLine("- **Legal Compliance**: Complying with applicable laws and regulations");
                writer.WriteLine();
                
                writer.WriteLine("## 4. Information Sharing and Disclosure");
                writer.WriteLine();
                writer.WriteLine("We do not sell, trade, or rent your personal information. We may share information in the following circumstances:");
                writer.WriteLine();
                writer.WriteLine("### 4.1 Service Providers");
                writer.WriteLine("- **AdMob**: For advertising services and analytics");
                writer.WriteLine("- **Analytics Providers**: For performance monitoring and crash reporting");
                writer.WriteLine("- **Cloud Storage**: For data backup and synchronization");
                writer.WriteLine();
                
                writer.WriteLine("### 4.2 Legal Requirements");
                writer.WriteLine("- Compliance with legal obligations");
                writer.WriteLine("- Protection of our rights and safety");
                writer.WriteLine("- Investigation of potential violations");
                writer.WriteLine();
                
                writer.WriteLine("### 4.3 Business Transfers");
                writer.WriteLine("In the event of a merger, acquisition, or sale of assets, your information may be transferred.");
                writer.WriteLine();
                
                writer.WriteLine("## 5. Data Security");
                writer.WriteLine();
                writer.WriteLine("We implement appropriate security measures to protect your information:");
                writer.WriteLine("- **Encryption**: Data transmission is encrypted using HTTPS");
                writer.WriteLine("- **Access Controls**: Limited access to personal information");
                writer.WriteLine("- **Regular Updates**: Security systems are regularly updated");
                writer.WriteLine("- **Monitoring**: Continuous monitoring for security threats");
                writer.WriteLine();
                
                writer.WriteLine("## 6. Data Retention");
                writer.WriteLine();
                writer.WriteLine("We retain your information for as long as necessary to provide the Service and fulfill the purposes outlined in this policy:");
                writer.WriteLine("- **Game Data**: Retained while your account is active");
                writer.WriteLine("- **Analytics Data**: Aggregated data may be retained for up to 2 years");
                writer.WriteLine("- **Support Communications**: Retained for up to 3 years");
                writer.WriteLine("- **Legal Requirements**: Some data may be retained longer as required by law");
                writer.WriteLine();
                
                writer.WriteLine("## 7. Your Rights and Choices");
                writer.WriteLine();
                writer.WriteLine("### 7.1 GDPR Rights (EU Users)");
                writer.WriteLine("- **Access**: Request copies of your personal data");
                writer.WriteLine("- **Rectification**: Request correction of inaccurate data");
                writer.WriteLine("- **Erasure**: Request deletion of your personal data");
                writer.WriteLine("- **Portability**: Request transfer of your data");
                writer.WriteLine("- **Objection**: Object to processing of your data");
                writer.WriteLine("- **Restriction**: Request restriction of processing");
                writer.WriteLine();
                
                writer.WriteLine("### 7.2 CCPA Rights (California Users)");
                writer.WriteLine("- **Know**: What personal information is collected");
                writer.WriteLine("- **Delete**: Request deletion of personal information");
                writer.WriteLine("- **Opt-Out**: Opt-out of sale of personal information");
                writer.WriteLine("- **Non-Discrimination**: Equal service regardless of privacy choices");
                writer.WriteLine();
                
                writer.WriteLine("### 7.3 General Rights");
                writer.WriteLine("- **Consent Withdrawal**: Withdraw consent at any time");
                writer.WriteLine("- **Data Portability**: Export your game data");
                writer.WriteLine("- **Account Deletion**: Delete your account and associated data");
                writer.WriteLine("- **Communication Preferences**: Opt-out of promotional communications");
                writer.WriteLine();
                
                writer.WriteLine("## 8. Children's Privacy");
                writer.WriteLine();
                writer.WriteLine("Our Service is not directed to children under 13. We do not knowingly collect personal information from children under 13. If you are a parent and believe your child has provided us with personal information, please contact us for deletion.");
                writer.WriteLine();
                
                writer.WriteLine("## 9. International Data Transfers");
                writer.WriteLine();
                writer.WriteLine("Your information may be transferred to and processed in countries other than your own. We ensure appropriate safeguards are in place for international transfers.");
                writer.WriteLine();
                
                writer.WriteLine("## 10. Changes to This Privacy Policy");
                writer.WriteLine();
                writer.WriteLine("We may update this Privacy Policy from time to time. We will notify you of any changes by posting the new Privacy Policy on this page and updating the \"Last Updated\" date.");
                writer.WriteLine();
                
                writer.WriteLine("## 11. Contact Information");
                writer.WriteLine();
                writer.WriteLine("If you have questions about this Privacy Policy, please contact us:");
                writer.WriteLine();
                writer.WriteLine("**Email**: privacy@miffgames.com");
                writer.WriteLine("**Website**: www.miffgames.com");
                writer.WriteLine("**Mail**:");
                writer.WriteLine("Miff Games");
                writer.WriteLine("Privacy Department");
                writer.WriteLine("[Address]");
                writer.WriteLine();
                
                writer.WriteLine("## 12. Advertising and Analytics");
                writer.WriteLine();
                writer.WriteLine("### 12.1 AdMob Integration");
                writer.WriteLine("Our game uses Google AdMob to display advertisements. AdMob may collect and use data to provide ads. For more information about AdMob's privacy practices, visit:");
                writer.WriteLine("- **AdMob Privacy Policy**: https://policies.google.com/privacy");
                writer.WriteLine("- **AdMob Settings**: https://adssettings.google.com/");
                writer.WriteLine();
                
                writer.WriteLine("### 12.2 Analytics");
                writer.WriteLine("We use analytics services to understand how users interact with our game:");
                writer.WriteLine("- **Performance Monitoring**: Crash reporting and error tracking");
                writer.WriteLine("- **User Behavior**: Understanding feature usage and game progression");
                writer.WriteLine("- **Optimization**: Improving game performance and user experience");
                writer.WriteLine();
                
                writer.WriteLine("## 13. Cookies and Tracking Technologies");
                writer.WriteLine();
                writer.WriteLine("We use various tracking technologies to collect and store information about your use of our Service. See our Cookie Policy for more details.");
                writer.WriteLine();
                
                writer.WriteLine("## 14. Compliance and Enforcement");
                writer.WriteLine();
                writer.WriteLine("We regularly review our privacy practices to ensure compliance with applicable laws and regulations. We may engage third-party auditors to verify compliance.");
                writer.WriteLine();
            }
            
            GD.Print($"Privacy policy created: {_privacyPolicyPath}");
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to create privacy policy: {e.Message}");
        }
    }

    /// <summary>
    /// Create terms of service
    /// </summary>
    private void CreateTermsOfService()
    {
        try
        {
            using (var writer = new StreamWriter(_termsOfServicePath))
            {
                writer.WriteLine("# Terms of Service");
                writer.WriteLine();
                writer.WriteLine("**Last Updated: January 6, 2025**");
                writer.WriteLine();
                
                writer.WriteLine("## 1. Acceptance of Terms");
                writer.WriteLine();
                writer.WriteLine("By downloading, installing, or using the Angry Animals game (the \"Service\"), you agree to be bound by these Terms of Service (\"Terms\"). If you disagree with any part of these terms, you may not use the Service.");
                writer.WriteLine();
                
                writer.WriteLine("## 2. Description of Service");
                writer.WriteLine();
                writer.WriteLine("Angry Animals is a physics-based puzzle game where players use slingshots to launch characters at targets. The Service includes:");
                writer.WriteLine("- Game gameplay and levels");
                writer.WriteLine("- Character customization features");
                writer.WriteLine("- Procedural level generation");
                writer.WriteLine("- Online features (leaderboards, daily challenges)");
                writer.WriteLine("- In-app purchases");
                writer.WriteLine();
                
                writer.WriteLine("## 3. User Accounts");
                writer.WriteLine();
                writer.WriteLine("### 3.1 Account Creation");
                writer.WriteLine("You may create an account to save your progress and access additional features. You are responsible for:");
                writer.WriteLine("- Maintaining the confidentiality of your account credentials");
                writer.WriteLine("- All activities that occur under your account");
                writer.WriteLine("- Providing accurate and complete information");
                writer.WriteLine();
                
                writer.WriteLine("### 3.2 Account Termination");
                writer.WriteLine("We reserve the right to terminate or suspend your account at any time for any reason, including violation of these Terms.");
                writer.WriteLine();
                
                writer.WriteLine("## 4. User Conduct");
                writer.WriteLine();
                writer.WriteLine("You agree not to:");
                writer.WriteLine("- Use the Service for any illegal purpose");
                writer.WriteLine("- Attempt to gain unauthorized access to the Service");
                writer.WriteLine("- Interfere with or disrupt the Service");
                writer.WriteLine("- Upload malicious code or harmful content");
                writer.WriteLine("- Harass, abuse, or harm other users");
                writer.WriteLine("- Violate any applicable laws or regulations");
                writer.WriteLine();
                
                writer.WriteLine("## 5. In-App Purchases");
                writer.WriteLine();
                writer.WriteLine("### 5.1 Virtual Currency and Items");
                writer.WriteLine("The Service may include virtual currency or items that can be purchased with real money. These items:");
                writer.WriteLine("- Are provided for entertainment purposes only");
                writer.WriteLine("- Have no cash value");
                writer.WriteLine("- Cannot be transferred or sold");
                writer.WriteLine("- May be modified or discontinued at any time");
                writer.WriteLine();
                
                writer.WriteLine("### 5.2 Refunds");
                writer.WriteLine("All purchases are final and non-refundable except as required by applicable law or as set forth in these Terms.");
                writer.WriteLine();
                
                writer.WriteLine("## 6. Intellectual Property");
                writer.WriteLine();
                writer.WriteLine("### 6.1 Our Rights");
                writer.WriteLine("The Service, including all content, features, and functionality, is owned by us and is protected by copyright, trademark, and other intellectual property laws.");
                writer.WriteLine();
                
                writer.WriteLine("### 6.2 User Content");
                writer.WriteLine("You retain ownership of content you create (such as custom character photos), but grant us a license to use, modify, and display such content in connection with the Service.");
                writer.WriteLine();
                
                writer.WriteLine("## 7. Privacy");
                writer.WriteLine();
                writer.WriteLine("Your privacy is important to us. Our collection and use of personal information is governed by our Privacy Policy, which is incorporated into these Terms by reference.");
                writer.WriteLine();
                
                writer.WriteLine("## 8. Disclaimers");
                writer.WriteLine();
                writer.WriteLine("### 8.1 Service Availability");
                writer.WriteLine("The Service is provided \"as is\" without warranties of any kind. We do not guarantee that the Service will be uninterrupted or error-free.");
                writer.WriteLine();
                
                writer.WriteLine("### 8.2 Third-Party Services");
                writer.WriteLine("We are not responsible for the availability, content, or practices of third-party services linked to or integrated with the Service.");
                writer.WriteLine();
                
                writer.WriteLine("## 9. Limitation of Liability");
                writer.WriteLine();
                writer.WriteLine("To the maximum extent permitted by law, we shall not be liable for any indirect, incidental, special, consequential, or punitive damages resulting from your use of the Service.");
                writer.WriteLine();
                
                writer.WriteLine("## 10. Indemnification");
                writer.WriteLine();
                writer.WriteLine("You agree to indemnify and hold us harmless from any claims, damages, or expenses arising from your use of the Service or violation of these Terms.");
                writer.WriteLine();
                
                writer.WriteLine("## 11. Changes to Terms");
                writer.WriteLine();
                writer.WriteLine("We may modify these Terms at any time. We will notify users of material changes through the Service or other means. Continued use of the Service constitutes acceptance of modified Terms.");
                writer.WriteLine();
                
                writer.WriteLine("## 12. Governing Law");
                writer.WriteLine();
                writer.WriteLine("These Terms shall be governed by and construed in accordance with the laws of [Jurisdiction], without regard to conflict of law principles.");
                writer.WriteLine();
                
                writer.WriteLine("## 13. Contact Information");
                writer.WriteLine();
                writer.WriteLine("For questions about these Terms, contact us:");
                writer.WriteLine();
                writer.WriteLine("**Email**: legal@miffgames.com");
                writer.WriteLine("**Website**: www.miffgames.com");
                writer.WriteLine("**Mail**:");
                writer.WriteLine("Miff Games");
                writer.WriteLine("Legal Department");
                writer.WriteLine("[Address]");
            }
            
            GD.Print($"Terms of service created: {_termsOfServicePath}");
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to create terms of service: {e.Message}");
        }
    }

    /// <summary>
    /// Create cookie policy
    /// </summary>
    private void CreateCookiePolicy()
    {
        try
        {
            using (var writer = new StreamWriter(_cookiePolicyPath))
            {
                writer.WriteLine("# Cookie Policy");
                writer.WriteLine();
                writer.WriteLine("**Last Updated: January 6, 2025**");
                writer.WriteLine();
                
                writer.WriteLine("## What Are Cookies?");
                writer.WriteLine();
                writer.WriteLine("Cookies are small data files stored on your device when you use the Angry Animals game. They help us provide and improve our services.");
                writer.WriteLine();
                
                writer.WriteLine("## How We Use Cookies");
                writer.WriteLine();
                writer.WriteLine("### Essential Cookies");
                writer.WriteLine("These cookies are necessary for the game to function:");
                writer.WriteLine("- **Session Management**: Keeping you logged in");
                writer.WriteLine("- **Game Progress**: Saving your game state");
                writer.WriteLine("- **Security**: Preventing fraudulent activity");
                writer.WriteLine();
                
                writer.WriteLine("### Analytics Cookies");
                writer.WriteLine("These cookies help us understand game performance:");
                writer.WriteLine("- **Usage Statistics**: Understanding which features are used");
                writer.WriteLine("- **Crash Reporting**: Identifying and fixing bugs");
                writer.WriteLine("- **Performance Monitoring**: Optimizing game performance");
                writer.WriteLine();
                
                writer.WriteLine("### Advertising Cookies");
                writer.WriteLine("These cookies are used by our advertising partners:");
                writer.WriteLine("- **AdMob**: Delivering relevant advertisements");
                writer.WriteLine("- **Performance Tracking**: Measuring ad effectiveness");
                writer.WriteLine("- **Frequency Capping**: Limiting how often you see the same ads");
                writer.WriteLine();
                
                writer.WriteLine("## Managing Cookies");
                writer.WriteLine();
                writer.WriteLine("You can control cookies through:");
                writer.WriteLine("- **Game Settings**: Opt-out of non-essential cookies");
                writer.WriteLine("- **Device Settings**: Browser or device privacy settings");
                writer.WriteLine("- **Opt-Out Tools**: Industry opt-out mechanisms");
                writer.WriteLine();
                
                writer.WriteLine("## Contact Us");
                writer.WriteLine();
                writer.WriteLine("For questions about cookies, contact us at privacy@miffgames.com");
            }
            
            GD.Print($"Cookie policy created: {_cookiePolicyPath}");
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to create cookie policy: {e.Message}");
        }
    }

    /// <summary>
    /// Create data retention policy
    /// </summary>
    private void CreateDataRetentionPolicy()
    {
        string retentionPath = "res://legal/DataRetentionPolicy.md";
        
        try
        {
            using (var writer = new StreamWriter(retentionPath))
            {
                writer.WriteLine("# Data Retention Policy");
                writer.WriteLine();
                writer.WriteLine("**Last Updated: January 6, 2025**");
                writer.WriteLine();
                
                writer.WriteLine("## Data Retention Periods");
                writer.WriteLine();
                writer.WriteLine("We retain different types of data for varying periods:");
                writer.WriteLine();
                writer.WriteLine("### Account Data");
                writer.WriteLine("- **Active Accounts**: Retained while account is active");
                writer.WriteLine("- **Inactive Accounts**: Deleted after 2 years of inactivity");
                writer.WriteLine("- **Deleted Accounts**: All data deleted within 30 days");
                writer.WriteLine();
                
                writer.WriteLine("### Game Analytics");
                writer.WriteLine("- **Aggregated Data**: Retained for 2 years");
                writer.WriteLine("- **Individual Sessions**: Retained for 1 year");
                writer.WriteLine("- **Crash Reports**: Retained for 1 year");
                writer.WriteLine();
                
                writer.WriteLine("### Support Communications");
                writer.WriteLine("- **Support Tickets**: Retained for 3 years");
                writer.WriteLine("- **User Feedback**: Retained for 2 years");
                writer.WriteLine("- **Bug Reports**: Retained indefinitely for product improvement");
                writer.WriteLine();
                
                writer.WriteLine("### Legal Requirements");
                writer.WriteLine("- **Financial Records**: Retained as required by law (typically 7 years)");
                writer.WriteLine("- **Compliance Data**: Retained as required by regulations");
                writer.WriteLine("- **Security Logs**: Retained for 1 year");
                writer.WriteLine();
            }
            
            GD.Print($"Data retention policy created: {retentionPath}");
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to create data retention policy: {e.Message}");
        }
    }

    /// <summary>
    /// Load consent data from file
    /// </summary>
    private void LoadConsentData()
    {
        string consentPath = "user://consent_data.json";
        
        try
        {
            if (File.Exists(consentPath))
            {
                string jsonContent = File.ReadAllText(consentPath);
                _consentData = JsonSerializer.Deserialize<ConsentData>(jsonContent) ?? new ConsentData();
            }
            else
            {
                _consentData = new ConsentData();
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to load consent data: {e.Message}");
            _consentData = new ConsentData();
        }
    }

    /// <summary>
    /// Save consent data to file
    /// </summary>
    private void SaveConsentData()
    {
        string consentPath = "user://consent_data.json";
        
        try
        {
            var options = new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            string json = JsonSerializer.Serialize(_consentData, options);
            File.WriteAllText(consentPath, json);
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to save consent data: {e.Message}");
        }
    }

    /// <summary>
    /// Show privacy notice dialog
    /// </summary>
    public void ShowPrivacyNotice()
    {
        if (_hasShownPrivacyNotice) return;
        
        var dialog = new AcceptDialog();
        dialog.Name = "PrivacyNoticeDialog";
        dialog.Title = "Privacy Notice";
        dialog.Size = new Vector2(600, 400);
        
        // Create scrollable content
        var scroll = new ScrollContainer();
        scroll.SizeFlagsVertical = Control.SizeFlags.Fill;
        scroll.SizeFlagsHorizontal = Control.SizeFlags.Fill;
        
        var vbox = new VBoxContainer();
        vbox.SizeFlagsVertical = Control.SizeFlags.Fill;
        vbox.SizeFlagsHorizontal = Control.SizeFlags.Fill;
        
        // Privacy notice content
        var label = new Label();
        label.Text = "We use cookies and collect data to provide and improve our services.\n\n" +
                    "• Essential cookies for game functionality\n" +
                    "• Analytics to improve game performance\n" +
                    "• Advertising to support free gameplay\n\n" +
                    "By continuing, you agree to our Privacy Policy and Terms of Service.\n\n" +
                    "You can change your preferences in the Settings menu.";
        label.AutowrapMode = TextServer.AutowrapMode.Word;
        label.SizeFlagsHorizontal = Control.SizeFlags.Fill;
        
        vbox.AddChild(label);
        scroll.AddChild(vbox);
        
        dialog.AddChild(scroll);
        
        dialog.Confirmed += () => {
            _consentData.HasConsent = true;
            _consentData.ConsentDate = DateTime.Now;
            _consentData.Version = "1.0";
            SaveConsentData();
            _hasShownPrivacyNotice = true;
            EmitSignal("PrivacyConsentChanged", true, _consentData);
        };
        
        dialog.Canceled += () => {
            _hasShownPrivacyNotice = true;
            // User can review privacy settings in menu
        };
        
        // Add to scene tree
        var viewport = GetTree().Root;
        viewport.AddChild(dialog);
        
        dialog.PopupCentered();
        
        EmitSignal("PrivacyPolicyViewed");
    }

    /// <summary>
    /// Update consent preferences
    /// </summary>
    public void UpdateConsentPreferences(bool analytics, bool advertising, bool personalization)
    {
        _consentData.AnalyticsConsent = analytics;
        _consentData.AdvertisingConsent = advertising;
        _consentData.PersonalizationConsent = personalization;
        _consentData.LastUpdated = DateTime.Now;
        
        SaveConsentData();
        EmitSignal("PrivacyConsentChanged", true, _consentData);
    }

    /// <summary>
    /// Check if user has given consent
    /// </summary>
    public bool HasConsent()
    {
        return _consentData.HasConsent;
    }

    /// <summary>
    /// Get current consent data
    /// </summary>
    public ConsentData GetConsentData()
    {
        return _consentData;
    }

    /// <summary>
    /// Show privacy policy in web view or dialog
    /// </summary>
    public void ShowPrivacyPolicy()
    {
        if (File.Exists(_privacyPolicyPath))
        {
            string content = File.ReadAllText(_privacyPolicyPath);
            ShowLegalDocument("Privacy Policy", content);
        }
    }

    /// <summary>
    /// Show terms of service
    /// </summary>
    public void ShowTermsOfService()
    {
        if (File.Exists(_termsOfServicePath))
        {
            string content = File.ReadAllText(_termsOfServicePath);
            ShowLegalDocument("Terms of Service", content);
        }
    }

    /// <summary>
    /// Display legal document in a dialog
    /// </summary>
    private void ShowLegalDocument(string title, string content)
    {
        var dialog = new AcceptDialog();
        dialog.Name = $"{title}Dialog";
        dialog.Title = title;
        dialog.Size = new Vector2(800, 600);
        
        // Create scrollable content
        var scroll = new ScrollContainer();
        scroll.SizeFlagsVertical = Control.SizeFlags.Fill;
        scroll.SizeFlagsHorizontal = Control.SizeFlags.Fill;
        
        var textEdit = new TextEdit();
        textEdit.Text = content;
        textEdit.Readonly = true;
        textEdit.SizeFlagsVertical = Control.SizeFlags.Fill;
        textEdit.SizeFlagsHorizontal = Control.SizeFlags.Fill;
        
        scroll.AddChild(textEdit);
        dialog.AddChild(scroll);
        
        // Add to scene tree
        var viewport = GetTree().Root;
        viewport.AddChild(dialog);
        
        dialog.PopupCentered();
    }

    /// <summary>
    /// Request data deletion
    /// </summary>
    public void RequestDataDeletion()
    {
        // This would typically involve:
        // 1. Mark account for deletion
        // 2. Remove from analytics systems
        // 3. Delete from databases
        // 4. Send confirmation
        
        GD.Print("Data deletion requested - would contact backend services");
    }
}

/// <summary>
/// Consent data structure
/// </summary>
public class ConsentData
{
    public bool HasConsent { get; set; } = false;
    public DateTime ConsentDate { get; set; } = DateTime.MinValue;
    public DateTime LastUpdated { get; set; } = DateTime.MinValue;
    public string Version { get; set; } = "1.0";
    public bool AnalyticsConsent { get; set; } = false;
    public bool AdvertisingConsent { get; set; } = false;
    public bool PersonalizationConsent { get; set; } = false;
    public bool LocationConsent { get; set; } = false;
    public bool NotificationConsent { get; set; } = false;
}