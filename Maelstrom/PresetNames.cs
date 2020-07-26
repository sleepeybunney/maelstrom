using System;
using System.Collections.Generic;
using System.Linq;
using FF8Mod.Archive;
using FF8Mod.Menu;

namespace FF8Mod.Maelstrom
{
    public class PresetNames
    {
        public static string ArchivePath = Globals.DataPath + @"\menu\mngrp.bin";
        public static int FileOffset = 0x2000;
        public static int FileLength = 0x2000;
        public static int NamesPage = 5;

        public static int Squall = 8;
        public static int Rinoa = 9;
        public static int Angelo = 10;
        public static int Quezacotl = 11;
        public static int Shiva = 12;
        public static int Ifrit = 13;
        public static int Siren = 14;
        public static int Brothers = 15;
        public static int Diablos = 16;
        public static int Carbuncle = 17;
        public static int Leviathan = 18;
        public static int Pandemona = 19;
        public static int Cerberus = 20;
        public static int Alexander = 21;
        public static int Doomtrain = 22;
        public static int Bahamut = 23;
        public static int Cactuar = 24;
        public static int Tonberry = 25;
        public static int Eden = 26;
        public static int Boko = 51;
        public static int Griever = 52;

        public static void Apply(FileSource menuSource)
        {
            // pull the relevant file out of the archive
            var file = menuSource.GetFile(ArchivePath);
            var mes3bytes = new ArraySegment<byte>(file, FileOffset, FileLength);
            var mes3 = TextFile.FromBytes(mes3bytes.ToArray(), true);

            // set names
            mes3.Pages[NamesPage].Strings[Squall] = Properties.Settings.Default.NameSquall;
            mes3.Pages[NamesPage].Strings[Rinoa] = Properties.Settings.Default.NameRinoa;
            mes3.Pages[NamesPage].Strings[Angelo] = Properties.Settings.Default.NameAngelo;
            mes3.Pages[NamesPage].Strings[Quezacotl] = Properties.Settings.Default.NameQuezacotl;
            mes3.Pages[NamesPage].Strings[Shiva] = Properties.Settings.Default.NameShiva;
            mes3.Pages[NamesPage].Strings[Ifrit] = Properties.Settings.Default.NameIfrit;
            mes3.Pages[NamesPage].Strings[Siren] = Properties.Settings.Default.NameSiren;
            mes3.Pages[NamesPage].Strings[Brothers] = Properties.Settings.Default.NameBrothers;
            mes3.Pages[NamesPage].Strings[Diablos] = Properties.Settings.Default.NameDiablos;
            mes3.Pages[NamesPage].Strings[Carbuncle] = Properties.Settings.Default.NameCarbuncle;
            mes3.Pages[NamesPage].Strings[Leviathan] = Properties.Settings.Default.NameLeviathan;
            mes3.Pages[NamesPage].Strings[Pandemona] = Properties.Settings.Default.NamePandemona;
            mes3.Pages[NamesPage].Strings[Cerberus] = Properties.Settings.Default.NameCerberus;
            mes3.Pages[NamesPage].Strings[Alexander] = Properties.Settings.Default.NameAlexander;
            mes3.Pages[NamesPage].Strings[Doomtrain] = Properties.Settings.Default.NameDoomtrain;
            mes3.Pages[NamesPage].Strings[Bahamut] = Properties.Settings.Default.NameBahamut;
            mes3.Pages[NamesPage].Strings[Cactuar] = Properties.Settings.Default.NameCactuar;
            mes3.Pages[NamesPage].Strings[Tonberry] = Properties.Settings.Default.NameTonberry;
            mes3.Pages[NamesPage].Strings[Eden] = Properties.Settings.Default.NameEden;
            mes3.Pages[NamesPage].Strings[Boko] = Properties.Settings.Default.NameBoko;
            mes3.Pages[NamesPage].Strings[Griever] = Properties.Settings.Default.NameGriever;

            // apply changes
            Array.Copy(mes3.Encode(), 0, file, FileOffset, FileLength);
            menuSource.ReplaceFile(ArchivePath, file);
        }
    }
}
