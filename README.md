# Maelstrom

An open-world randomiser for Final Fantasy VIII

## Features

* Shuffle boss locations, with or without duplicates
* Buy random, potentially powerful items from shops
* Teach your GF a randomised set of abilities, including item-exclusives
* Win rare Triple Triad cards from players all over the world
* Receive random spells from draw points, including Slots-exclusives and cut content
* Change the battle music to something very annoying & instantly regret it
* Pre-set your character & GF names - a little extra convenience for multiple playthroughs & speedruns
* Generate a spoiler file containing all boss locations, shop inventories, etc. just in case you get lost

## Free Roam (pre-alpha)

Inspired by [FF4: Free Enterprise](https://ff4fe.com), this mode will allow you to travel the world as you see fit, right from the start of the game. You get two party members and two GFs, and the rest is up to you. Each boss you find & defeat will grant major rewards such as new characters, new vehicles, and unsealed powers in Ultimecia Castle.

Free Roam is roughly 10% complete so far. You can fight some bosses around Balamb (step into the black clouds) and reach the western continent by train, but there is still a lot of work to be done: clearing out story-related obstacles, placing new triggers for boss encounters, and so on. Hopefully there's enough to demo the concept at least. Try it out & let me know what you think!

## Requirements

* Final Fantasy VIII - Remastered or the 2013 Steam release
* No other mods installed (some things MAY work but no testing has been done - if you just want to fix the MIDI music you can use [this](https://steamcommunity.com/app/39150/discussions/0/35221031741516824/))
* Windows 7 or above with the latest .NET Framework
* 2GB free disk space (for backups & working files)

## Installation & Usage

* Download the [current release](https://github.com/sleepeybunney/maelstrom/releases) or the latest [pre-release build](https://github.com/sleepeybunney/maelstrom/actions)
* Extract all files to any folder & run Maelstrom.exe
* On the "Config" tab browse to where your game is installed and select FFVIII.exe (for Remastered) or FF8_xx.exe (for vanilla, where 'xx' represents your game's language code, such as EN for English)
* Check the boxes on each tab for the features you want to activate, or select a pre-configured game from the "Presets" list
* Click "Go" to mod the game - this may take a few minutes depending on your PC & chosen settings
* To remove all changes and restore the game to normal, select the "Vanilla" preset & click "Go"

## Notes

* The mod works by patching game files. You don't need to keep the Maelstrom window open after it's finished processing, and you don't ever need to run it again unless you want to re-roll.

* Saving and loading all works as it should, as long as you don't re-roll in between. It'll still load, but you'll be on the new roll instead of the one you saved. To go back to a previous playthrough, run Maelstrom with the same seed & settings as before.

* The first time you run the randomiser it will make backups (.bak) of various files in the FF8 folder tree -- don't touch those!! If they get lost or corrupted you will need to restore/reinstall the game through Steam before running Maelstrom again.

* If "Set Seed" is unchecked, a new seed number will be generated for each run. Checking the box will allow you to enter your own. You can find a list of seeds you've used on the "History" tab, most recent at the top. (There are many explanations online for what an RNG "seed" is, but basically if you use the same number twice with the same settings, you will get the same results from the randomiser both times)

* Card locations in the spoiler file are currently incomplete. The field names are all there but the NPC descriptions are largely unhelpful.

* Some features aren't fully implemented yet for the Remastered version of the game. These are noted in *italics* beneath the relevant section.

* Efforts have been made to eliminate softlocks and crashes, but this is a work in progress and certain things may be broken or simply not yet complete. If you find yourself unable to progress and do not wish to re-roll:
  
  * Close the game and run Maelstrom

  * Save your settings on the "Presets" tab

  * Load the "Vanilla" preset and click "Go" to reset the game to normal

  * Run the game, play past the part you are stuck on, and save

  * Close it and run Maelstrom again

  * Restore your settings ("Presets" tab) and seed number ("History" tab), then click "Go"

## Planned Features

* Support for more versions of the game, such as PSX, PC CD-ROM, & Switch
* Shuffled GF locations
* Shuffled monster locations

If you have ideas for new features or improvements, feel free to message me here or on [Twitter](https://twitter.com/sleepeybunney)!

## Credits

None of this would have been possible without dozens of [QhimmWiki](https://wiki.ffrtt.ru/index.php/FF8) contributors and forum posters who are all a lot smarter than me!!

Big thanks to **Bosshunter** for coming up with ideas, helping me figure stuff out & just generally being a pal!!!

### Tools and references used

* [Battle script writeup](http://pingval.g1.xrea.com/psff8/research/index_en.html#enemy-ai) by pingval
* [Deling](https://github.com/myst6re/deling) by myst6re
* [Ifrit](https://sourceforge.net/projects/ifrit/) by gjoerulv
* [Cactilio](http://forums.qhimm.com/index.php?topic=16275.0) by JeMaCheHi
* [zzzDeArchive](https://github.com/Sebanisu/zzzDeArchive) by Sebanisu
* [Doomtrain](https://github.com/alexfilth/doomtrain) by alexfilth
