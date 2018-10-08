using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Threading;

namespace MPP.core
{
    enum Language
    {
        PL = 0,
        ENG = 1
    }

    class ResourceManager
    {
        Language lang;

        static private ResourceManager instance;
        static public ResourceManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ResourceManager();
                }
                return instance;
            }
        }

        private Dictionary<Language, Dictionary<StringKey, String>> strings;

        private ResourceManager()
        {
            GetLanguage();

            strings = new Dictionary<Language, Dictionary<StringKey, String>>();

            Dictionary<StringKey, String> stringsPL = new Dictionary<StringKey,string>();
            Dictionary<StringKey, String> stringsEN = new Dictionary<StringKey,string>();

            stringsPL.Add(StringKey.LEVEL, "Poziom ");
            stringsPL.Add(StringKey.LIVES, "Zycia");
            stringsPL.Add(StringKey.SCORE, "Punkty");
            stringsPL.Add(StringKey.UPGRADE_LEVEL, "MOC ");

            stringsPL.Add(StringKey.LEVEL_SUMMARY, "Twoj poziom");
            stringsPL.Add(StringKey.SCORE_SUMMARY, "Twoje punkty");
            stringsPL.Add(StringKey.HELP_SCREEN_BACKGROUND, "helpScreenPL");

            stringsPL.Add(StringKey.HIGHSCORE_SUMMARY, "Rekord");
            stringsPL.Add(StringKey.NEW_HIGHSCORE_SUMMARY, "NOWY REKORD");
            stringsPL.Add(StringKey.PREVIOUS_HIGHSCORE_SUMMARY, "Poprzedni");

            stringsEN.Add(StringKey.LEVEL, "Level ");
            stringsEN.Add(StringKey.LIVES, "Lives");
            stringsEN.Add(StringKey.SCORE, "Score");
            stringsEN.Add(StringKey.LEVEL_SUMMARY, "Final level");
            stringsEN.Add(StringKey.SCORE_SUMMARY, "Score");

            stringsEN.Add(StringKey.HIGHSCORE_SUMMARY, "High score");
            stringsEN.Add(StringKey.NEW_HIGHSCORE_SUMMARY, "NEW RECORD");
            stringsEN.Add(StringKey.PREVIOUS_HIGHSCORE_SUMMARY, "Last record");

            stringsPL.Add(StringKey.START_BATTLE, "Do boju!");
            stringsPL.Add(StringKey.START_A_NEW_GAME, "Nowa gra");
            stringsPL.Add(StringKey.RESUME_THE_GAME, "Kontynuuj gre");
            stringsPL.Add(StringKey.SOUND_ON, "Dzwiek: WL");
            stringsPL.Add(StringKey.SOUND_OFF, "Dzwiek: WYL");
            stringsPL.Add(StringKey.HELP, "Jak grac ?");
            stringsPL.Add(StringKey.MAIN_MENU_BUTTON_LABEL, "Powrot do menu");

            stringsEN.Add(StringKey.START_BATTLE, "Press to start");
            stringsEN.Add(StringKey.START_A_NEW_GAME, "New game");
            stringsEN.Add(StringKey.SOUND_ON, "Disable sound");
            stringsEN.Add(StringKey.SOUND_OFF, "Enable sound");
            stringsEN.Add(StringKey.HELP, "How to play ?");
            stringsEN.Add(StringKey.MAIN_MENU_BUTTON_LABEL, "Press to go back");
            stringsEN.Add(StringKey.RESUME_THE_GAME, "Resume");
            stringsEN.Add(StringKey.UPGRADE_LEVEL, "POWER ");
            stringsEN.Add(StringKey.HELP_SCREEN_BACKGROUND, "helpScreenEN");
            
            strings.Add(Language.PL, stringsPL);
            strings.Add(Language.ENG, stringsEN);
        }

        public Language GetLanguage()
        {
            /*
            CultureInfo ci = Thread.CurrentThread.CurrentCulture;
            if (ci.Name.Equals("pl-PL") || ci.Name.Equals("pl"))
            {
                lang = Language.PL;
            }
            else
            {*/
                SetLanguage(Language.ENG);
                lang = Language.ENG;
            //}

            return lang;
        }

        public void SetLanguage(Language lang)
        {
            if (lang == Language.ENG)
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            }
            else
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("pl-PL");
            }
        }

        public enum StringKey
        {
            LIVES,
            LEVEL,
            SCORE,
            UPGRADE_LEVEL,
            LEVEL_SUMMARY,
            SCORE_SUMMARY,
            HIGHSCORE_SUMMARY,
            NEW_HIGHSCORE_SUMMARY,
            PREVIOUS_HIGHSCORE_SUMMARY,
            START_A_NEW_GAME,
            RESUME_THE_GAME,
            START_BATTLE,
            MAIN_MENU_BUTTON_LABEL,
            SOUND_ON,
            SOUND_OFF,
            HELP,
            HELP_SCREEN_BACKGROUND
        }

        public String GetString(StringKey key)
        {
            Dictionary<StringKey, String> stringTable;
            if (strings.TryGetValue(GetLanguage(), out stringTable) == false)
            {
                throw new InvalidOperationException("Language not known");
            }

            return stringTable[key];
        }

        /* TODO */
    }
}