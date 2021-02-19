using System;
using System.Collections.Generic;
using System.Linq;
using Sleepey.FF8Mod;
using Sleepey.FF8Mod.Archive;
using Sleepey.FF8Mod.Menu;

namespace Sleepey.Maelstrom
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

        public static void Apply(FileSource menuSource, State settings)
        {
            // pull the relevant file out of the archive
            var file = menuSource.GetFile(ArchivePath);
            var mes3bytes = new ArraySegment<byte>(file, FileOffset, FileLength);
            var mes3 = TextFile.FromBytes(mes3bytes.ToArray(), true);

            // set names
            mes3.Pages[NamesPage].Strings[Squall] = settings.NameSquall;
            mes3.Pages[NamesPage].Strings[Rinoa] = settings.NameRinoa;
            mes3.Pages[NamesPage].Strings[Angelo] = settings.NameAngelo;
            mes3.Pages[NamesPage].Strings[Quezacotl] = settings.NameQuezacotl;
            mes3.Pages[NamesPage].Strings[Shiva] = settings.NameShiva;
            mes3.Pages[NamesPage].Strings[Ifrit] = settings.NameIfrit;
            mes3.Pages[NamesPage].Strings[Siren] = settings.NameSiren;
            mes3.Pages[NamesPage].Strings[Brothers] = settings.NameBrothers;
            mes3.Pages[NamesPage].Strings[Diablos] = settings.NameDiablos;
            mes3.Pages[NamesPage].Strings[Carbuncle] = settings.NameCarbuncle;
            mes3.Pages[NamesPage].Strings[Leviathan] = settings.NameLeviathan;
            mes3.Pages[NamesPage].Strings[Pandemona] = settings.NamePandemona;
            mes3.Pages[NamesPage].Strings[Cerberus] = settings.NameCerberus;
            mes3.Pages[NamesPage].Strings[Alexander] = settings.NameAlexander;
            mes3.Pages[NamesPage].Strings[Doomtrain] = settings.NameDoomtrain;
            mes3.Pages[NamesPage].Strings[Bahamut] = settings.NameBahamut;
            mes3.Pages[NamesPage].Strings[Cactuar] = settings.NameCactuar;
            mes3.Pages[NamesPage].Strings[Tonberry] = settings.NameTonberry;
            mes3.Pages[NamesPage].Strings[Eden] = settings.NameEden;
            mes3.Pages[NamesPage].Strings[Boko] = settings.NameBoko;
            mes3.Pages[NamesPage].Strings[Griever] = settings.NameGriever;

            // apply changes
            Array.Copy(mes3.Encode(), 0, file, FileOffset, FileLength);
            menuSource.ReplaceFile(ArchivePath, file);
        }
    }
}
