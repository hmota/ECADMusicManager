﻿namespace SincronizacaoMusical.UI
{
    public class ComboBoxPairs
    {
        public string monthName { get; set; }
        public int monthNumber { get; set; }

        public ComboBoxPairs(string MonthName, int MonthNumber)
        {
            monthName = MonthName;
            monthNumber = MonthNumber;
        }
    }
}
