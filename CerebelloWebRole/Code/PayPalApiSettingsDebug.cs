using PayPal.Version940;

namespace CerebelloWebRole.Code
{
    public class PayPalApiSettingsDebug : PayPalApiSettingsFromConfigurationManager
    {
        protected override string GetConfigKeyName(string key, string group)
        {
            if (key == PropertyNames.SettingsToUse)
                return base.GetConfigKeyName("Debug" + key, group);

            return base.GetConfigKeyName(key, group);
        }
    }
}