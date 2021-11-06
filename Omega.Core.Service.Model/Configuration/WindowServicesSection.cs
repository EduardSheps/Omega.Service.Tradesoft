using System;
using System.Configuration;
using System.ServiceProcess;

namespace Omega.Core.Service.Model.Configuration
{
    /// <summary>
    /// Реализует <see cref="ConfigurationSection"/> для настройки регистрации сервиса
    /// </summary>
    public class WindowServicesSection
        : ConfigurationSection
    {
        private const string InvalidUserNameChars = "\"<>|\0\x0001\x0002\x0003\x0004\x0005\x0006\a\b\t\n\v\f\r\x000e\x000f\x0010\x0011\x0012\x0013\x0014\x0015\x0016\x0017\x0018\x0019\x001a\x001b\x001c\x001d\x001e\x001f:*?\\/";

        private const string ACCOUNT_DEFAULT_VALUE = "NetworkService";
        private const string STARTMODE_DEFAULT_VALUE = "Automatic";

        private const string ACCOUNT_ELEMENT_NAME = "serviceAccount";
        public const string USERNAME_ELEMENT_NAME = "userName";
        internal const string PASSWORD_ELEMENT_NAME = "password";

        internal const string DELAYEDAUTOSTART_ELEMENT_NAME = "delayedAutoStart";
        internal const string DESCRIPTION_ELEMENT_NAME = "description";
        internal const string DISPLAYNAME_ELEMENT_NAME = "displayName";
        internal const string SERVICENAME_ELEMENT_NAME = "serviceName";
        internal const string STARTMODE_ELEMENT_NAME = "startMode";


        private readonly static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();

        private static readonly ConfigurationProperty _accountElement = new ConfigurationProperty(ACCOUNT_ELEMENT_NAME, typeof(string), ACCOUNT_DEFAULT_VALUE, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _userNameElement = new ConfigurationProperty(USERNAME_ELEMENT_NAME, typeof(string), string.Empty, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _passwordElement = new ConfigurationProperty(PASSWORD_ELEMENT_NAME, typeof(string), string.Empty, ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _delayedAutoStartElement = new ConfigurationProperty(DELAYEDAUTOSTART_ELEMENT_NAME, typeof(bool), false, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _descriptionElement = new ConfigurationProperty(DESCRIPTION_ELEMENT_NAME, typeof(string), string.Empty, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _displayNameElement = new ConfigurationProperty(DISPLAYNAME_ELEMENT_NAME, typeof(string), string.Empty, ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty _serviceNameElement = new ConfigurationProperty(SERVICENAME_ELEMENT_NAME, typeof(string), string.Empty, ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty _startModeElement = new ConfigurationProperty(STARTMODE_ELEMENT_NAME, typeof(string), STARTMODE_DEFAULT_VALUE, ConfigurationPropertyOptions.None);

        static WindowServicesSection()
        {
            _properties.Add(_accountElement);
            _properties.Add(_userNameElement);
            _properties.Add(_passwordElement);

            _properties.Add(_delayedAutoStartElement);
            _properties.Add(_descriptionElement);
            _properties.Add(_displayNameElement);
            _properties.Add(_serviceNameElement);
            _properties.Add(_startModeElement);
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }

        /// <summary>
        /// Получает или задает тип учетной записи, под управлением которой должно запускаться данное служебное приложение
        /// </summary>
        [RegexStringValidator(@"(?i)^(LocalService|NetworkService|LocalSystem|User)$")]
        public ServiceAccount ServiceAccount
        {
            get
            {
                ServiceAccount result;
                return (Enum.TryParse<ServiceAccount>(Convert.ToString(this[ACCOUNT_ELEMENT_NAME]), out result))
                    ? result

                    : ServiceAccount.NetworkService;
            }
            set
            {
                this[ACCOUNT_ELEMENT_NAME] = value.ToString("G");
            }
        }
        /// <summary>
        /// Получает или задает учетную запись пользователя, под управлением которой будет запускаться служебное приложение.
        /// </summary>
        [StringValidator(InvalidCharacters = InvalidUserNameChars, MinLength = 3, MaxLength = 512)]
        public string Username { get { return Convert.ToString(this[USERNAME_ELEMENT_NAME]); } set { this[USERNAME_ELEMENT_NAME] = value; } }
        /// <summary>
        /// Получает или задает пароль, связанный с учетной записью пользователя, под управлением которой запускается служебное приложение.
        /// </summary>
        [StringValidator(MinLength = 3, MaxLength = 512)]
        public string Password { get { return Convert.ToString(this[PASSWORD_ELEMENT_NAME]); } set { this[PASSWORD_ELEMENT_NAME] = value; } }

        /// <summary>
        /// Получает или задает значение, указывающее, следует ли задержать запуск службы, пока не заработают другие автоматически запускающиеся службы.
        /// </summary>
        public bool DelayedAutoStart { get { return Convert.ToBoolean(this[DELAYEDAUTOSTART_ELEMENT_NAME]); } set { this[DELAYEDAUTOSTART_ELEMENT_NAME] = value; } }
        /// <summary>
        /// Получает или задает описание для службы.
        /// </summary>
        [StringValidator(MinLength = 1, MaxLength = 2048)]
        public string Description { get { return Convert.ToString(this[DESCRIPTION_ELEMENT_NAME]); } set { this[DESCRIPTION_ELEMENT_NAME] = value; } }
        /// <summary>
        /// Определяет понятное имя, идентифицирующее службу для пользователя.
        /// </summary>
        [StringValidator(MinLength = 1, MaxLength = 128)]
        public string DisplayName { get { return Convert.ToString(this[DISPLAYNAME_ELEMENT_NAME]); } set { this[DISPLAYNAME_ELEMENT_NAME] = value; } }

        /// <summary>
        /// Определяет понятное имя, идентифицирующее службу для пользователя.
        /// </summary>
        [RegexStringValidator(@"^[A-Za-z() ]{1,256}$")]
        public string ServiceName { get { return Convert.ToString(this[SERVICENAME_ELEMENT_NAME]); } set { this[SERVICENAME_ELEMENT_NAME] = value; } }

        /// <summary>
        /// Получает или задает тип учетной записи, под управлением которой должно запускаться данное служебное приложение
        /// </summary>
        [RegexStringValidator(@"(?i)^(Automatic|Disabled|Manual)$")]
        public ServiceStartMode StartMode
        {
            get
            {
                ServiceStartMode result;
                return (Enum.TryParse<ServiceStartMode>(Convert.ToString(this[STARTMODE_ELEMENT_NAME]), out result))
                    ? result
                    : ServiceStartMode.Automatic;
            }
            set
            {
                this[STARTMODE_ELEMENT_NAME] = value.ToString("G");
            }
        }
    }
}
