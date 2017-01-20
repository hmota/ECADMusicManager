using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SincronizacaoMusical.Util
{
    public static class Tratamento
    {
        private static string RemoverAcentos(string texto)
        {
            string s = texto.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();

            for (int k = 0; k < s.Length; k++)
            {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(s[k]);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(s[k]);
                }
            }
            return sb.ToString();
        }

        public static string RemoveCaracteresEspeciais(string texto)
        {
            var strOut = string.Empty;
            texto = texto.Replace("'", " ").Replace('"', ' ');
            strOut = Regex.Replace(
                texto,
                @"[^0-9a-zA-ZéúíóáÉÚÍÓÁèùìòàÈÙÌÒÀõãñÕÃÑêûîôâÊÛÎÔÂëÿüïöäËYÜÏÖÄçÇ\s\.\!\?\(\)\{\}\[\]]+?",
                "-");
            strOut = Regex.Replace(strOut, @"\s+", " ");
            return strOut;
        }

        public static string Normalizar(this string texto)
        {
            var normal = RemoverAcentos(texto);
            normal = RemoveCaracteresEspeciais(normal);
            return normal.ToUpper().TrimStart();
        }

        public static string NormalizarTrim(this string texto)
        {
            var normal = RemoverAcentos(texto);
            normal = RemoveCaracteresEspeciais(normal);
            return normal.ToUpper().Trim();
            return normal.ToUpper();
        }

        public static bool CNPJ(this string CNPJ)
        {
            string Cnpj_1 = CNPJ.Substring(0, 12);
            string Cnpj_2 = CNPJ.Substring(CNPJ.Length - 2);
            string Mult = "543298765432";

            string Controle = String.Empty;
            int Digito = 0;
            for (int j = 1; j < 3; j++)
            {

                int Soma = 0;
                for (int i = 0; i < 12; i++)
                {
                    Soma += Convert.ToInt32(Cnpj_1.Substring(i, 1)) * Convert.ToInt32(Mult.Substring(i, 1));
                }

                if (j == 2)
                {
                    Soma += (2 * Digito);
                }

                Digito = ((Soma * 10) % 11);

                if (Digito == 10)
                {
                    Digito = 0;
                }

                Controle = Controle + Digito.ToString();
                Mult = "654329876543";
            }

            return (Controle != Cnpj_2);
        }

    }
}
