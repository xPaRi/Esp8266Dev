using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace EspComLib
{
    /// <summary>
    /// Třída pro zpracování příkazové řádky.
    /// </summary>
    /// <remarks>
    /// Oddělovač názvu parametru a jeho hodnoty je dvojtečka.
    /// Součástí názvu parametru nebo přepínače je i lomítko, případně něco jiného.
    /// </remarks>
    /// <example>
    /// MyProg /f:file.exe /sw1
    /// </example>
    public class MyCommandLine
    {
        /// <summary>
        /// Indikuje, zda jsou přepínače Case Sensitive.
        /// </summary>
        public bool IsCaseSensitive { get; }

        /// <summary>
        /// Provider pro převody hodnot z řetězce na specifický datový typ (double, decimal, int, ...).
        /// </summary>
        public IFormatProvider FormatProvider { get; }

        /// <summary>
        /// Seznam parametrů.
        /// </summary>
        public Dictionary<string, string> ParameterList { get; } = new Dictionary<string, string>();

        /// <summary>
        /// Konstruktor.
        /// </summary>
        /// <param name="args">Argumenty přílazové řádky.</param>
        /// <param name="isCaseSensitive">Indikuje, zda budou přepínače Case Sensitive. Default = false</param>
        /// <param name="formatProvider">Provider pro převody hodnot z řetězce na specifický datový typ (double, decimal, int, ...). Default = CultureInfo.InvariantCulture</param>
        public MyCommandLine(IEnumerable<string> args, bool isCaseSensitive = false, IFormatProvider formatProvider = null)
        {
            IsCaseSensitive = isCaseSensitive;
            FormatProvider = formatProvider ?? CultureInfo.InvariantCulture;

            foreach (var arg in args)
            {
                var keyVal = arg.Split(new[] { ':' }, 2);

                var key = CaseValidKey(keyVal[0]);

                if (ParameterList.ContainsKey(key))
                    continue;

                switch (keyVal.Length)
                {
                    case 1:
                        ParameterList.Add(key, true.ToString()); //přepínače nemají hodnotu, ale dáme True
                        break;
                    case 2:
                        ParameterList.Add(key, keyVal[1]);
                        break;
                }
            }
        }

        /// <summary>
        /// Signalizuje existenci zadaného klíče v parametrech.
        /// </summary>
        /// <param name="key">Kontrolovaný klíč.</param>
        /// <returns></returns>
        public bool Exists(string key)
        {
            return ParameterList.ContainsKey(CaseValidKey(key));
        }

        /// <summary>
        /// Signalizuje existenci alespoň jednoho ze zadaných klíčů v parametrech.
        /// </summary>
        /// <param name="keyList">Seznam kontrolovaných klíčů.</param>
        /// <returns></returns>
        public bool ExistsAny(IEnumerable<string> keyList)
        {
            return keyList.Any(Exists);
        }

        /// <summary>
        /// Signalizuje existenci všech zadaných klíčů v parametrech.
        /// </summary>
        /// <param name="keyList">Seznam kontrolovaných klíčů.</param>
        /// <returns></returns>
        public bool ExistsAll(IEnumerable<string> keyList)
        {
            return keyList.All(Exists);
        }

        /// <summary>
        /// Vrací hodnotu parametru jako String. Pokud neexistuje vrací 'defaultValue'.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public string Value(string key, string defaultValue)
        {
            return ParameterList.TryGetValue(CaseValidKey(key), out var value) ? value : defaultValue;
        }

        /// <summary>
        /// Vrací hodnotu parametru jako String. Pokud neexistuje vrací 'string.Empty'.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string Value(string key)
        {
            return Value(key, string.Empty);
        }

        /// <summary>
        /// Vrací hodnotu parametru jako Boolean. Pokud neexistuje vrací 'defaultValue'.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public bool Value(string key, bool defaultValue)
        {
            return bool.TryParse(Value(key, defaultValue.ToString()), out var value) ? value : defaultValue;
        }

        /// <summary>
        /// Vrací hodnotu parametru jako int. Pokud neexistuje vrací 'defaultValue'.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public int Value(string key, int defaultValue)
        {
            return int.TryParse(Value(key, defaultValue.ToString()), out var value) ? value : defaultValue;
        }

        /// <summary>
        /// Vrací hodnotu parametru jako Decimal. Pokud neexistuje vrací 'defaultValue'.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public decimal Value(string key, decimal defaultValue)
        {
            return decimal.TryParse(Value(key, defaultValue.ToString(FormatProvider)), NumberStyles.Any, FormatProvider, out var value) ? value : defaultValue;
        }

        /// <summary>
        /// Vrací hodnotu parametru jako Double. Pokud neexistuje vrací 'defaultValue'.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public double Value(string key, double defaultValue)
        {
            return double.TryParse(Value(key, defaultValue.ToString(FormatProvider)), NumberStyles.Any, FormatProvider, out var value) ? value : defaultValue;
        }

        #region Helpers

        /// <summary>
        /// Upravení klíče na správnou velikost podle nastavení CaseSensitive
        /// </summary>
        /// <param name="key">Klíč</param>
        /// <returns>Upravený klíč</returns>
        private string CaseValidKey(string key)
        {
            return (IsCaseSensitive) ? key : key.ToUpper();
        }

        #endregion //Helpers
    }
}
