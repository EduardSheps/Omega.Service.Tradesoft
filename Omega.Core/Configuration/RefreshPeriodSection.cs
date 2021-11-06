using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omega.Core.Configuration
{
    /// <summary>
    /// Описывает секцию настройки для обработки информации о периодических запросов к базе
    /// </summary>
    public class RefreshPeriodSection
        : ConfigurationSection
    {
        public const string DEFAULT_SECTION_NAME = "refreshPeriod";

        internal const string PERIOD_SECTION_NAME = "period";

        private readonly static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _periodElement = new ConfigurationProperty(
            PERIOD_SECTION_NAME, typeof(TimeSpan), new TimeSpan(0, 0, 5), new InfiniteTimeSpanConverter(), new TimeSpanValidator(new TimeSpan(0, 0, 1), new TimeSpan(2, 0, 0, 0)), ConfigurationPropertyOptions.IsRequired);

        static RefreshPeriodSection()
        {
            _properties.Add(_periodElement);
        }
        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }
        /// <summary>
        /// Возвращает значение временного интервала между вызовами методов
        /// </summary>
        public TimeSpan Period { get { return (TimeSpan)this[PERIOD_SECTION_NAME]; } set { this[PERIOD_SECTION_NAME] = value; } }

        #region Section

        private static readonly Dictionary<string, RefreshPeriodSection> _sectionDictinary = new Dictionary<string, RefreshPeriodSection>();


        /// <summary>
        /// Возвращает секцию настройки для обработки информации о периодических запросах к базе
        /// </summary>
        /// <summary>
        /// Возвращает секцию настройки для обработки информации о периодических запросах к базе
        /// </summary>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        public static RefreshPeriodSection SectionByName(string sectionName)
        {
            return SectionByName(sectionName, new TimeSpan(0, 5, 0));
        }
        /// <summary>
        /// Возвращает секцию настройки для обработки информации о периодических запросах к базе
        /// </summary>
        /// <summary>
        /// Возвращает секцию настройки для обработки информации о периодических запросах к базе
        /// </summary>
        /// <param name="sectionName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static RefreshPeriodSection SectionByName(string sectionName, TimeSpan defaultValue)
        {
            RefreshPeriodSection refreshPeriodSection;
            if (!_sectionDictinary.TryGetValue(sectionName, out refreshPeriodSection))
            {
                System.Configuration.Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                refreshPeriodSection = config.Sections[sectionName] as RefreshPeriodSection;
                if (refreshPeriodSection == null)
                {
                    refreshPeriodSection = new RefreshPeriodSection();
                    config.Sections.Add(sectionName, refreshPeriodSection);
                    refreshPeriodSection.SectionInformation.ForceSave = true;
                    refreshPeriodSection.Properties.Add(new ConfigurationProperty(PERIOD_SECTION_NAME, typeof(TimeSpan), defaultValue));
                    config.Save(ConfigurationSaveMode.Full);
                }
                _sectionDictinary.Add(sectionName, refreshPeriodSection);
            }
            return refreshPeriodSection;

        }
        #endregion Section

        /// <summary>
        /// Изменяет значение в настройках
        /// </summary>
        /// <param name="period">Новое значение периода</param>
        public static void SaveNewValue(TimeSpan period)
        {
            SaveNewValue(DEFAULT_SECTION_NAME, period);
        }
        /// <summary>
        /// Изменяет значение в настройках
        /// </summary>
        /// <param name="sectionName">Название секции</param>
        /// <param name="period">>Новое значение периода</param>
        public static void SaveNewValue(string sectionName, TimeSpan period)
        {
            RefreshPeriodSection refreshPeriodSection = SectionByName(sectionName);
            refreshPeriodSection.Period = period;
            if (refreshPeriodSection.CurrentConfiguration != null)
                refreshPeriodSection.CurrentConfiguration.Save(ConfigurationSaveMode.Full);
        }
    }
}
