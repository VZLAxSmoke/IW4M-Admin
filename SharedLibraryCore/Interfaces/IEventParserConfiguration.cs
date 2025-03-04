﻿namespace SharedLibraryCore.Interfaces
{
    public interface IEventParserConfiguration
    {
        /// <summary>
        /// stores the fs_game directory (this folder may vary between different clients)
        /// </summary>
        string GameDirectory { get; set; }
        /// <summary>
        /// stores the regex information for a say event printed in the game log
        /// </summary>
        ParserRegex Say { get; set; }
        /// <summary>
        /// stores the regex information for a join event printed in the game log
        /// </summary>
        ParserRegex Join { get; set; }
        /// <summary>
        /// stores the regex information for a quit event printed in the game log
        /// </summary>
        ParserRegex Quit { get; set; }
        /// <summary>
        /// stores the regex information for a kill event printed in the game log
        /// </summary>
        ParserRegex Kill { get; set; }
        /// <summary>
        /// stores the regex information for a damage event printed in the game log
        /// </summary>
        ParserRegex Damage { get; set; }
        /// <summary>
        /// stores the regex information for an action event printed in the game log
        /// </summary>
        ParserRegex Action { get; set; }
    }
}
