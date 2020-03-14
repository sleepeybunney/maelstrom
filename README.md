# Maelstrom

An open-world randomiser for Final Fantasy VIII

## Features

* Shuffle boss locations with optional level scaling
* Buy random, potentially powerful items from shops
* Win rare Triple Triad cards from players all over the world
* Receive random spells from draw points, including Slots-exclusives and cut content
* Pre-set your character & GF names - a little extra convenience for multiple playthroughs & speedruns
* Generate a spoiler file containing all boss locations, shop inventories, etc. just in case you get lost

## Free Roam

Inspired by [FF4: Free Enterprise](https://ff4fe.com), this mode will allow you to travel the world as you see fit, right from the start of the game. You get two party members and two GFs, and the rest is up to you. Each boss you find & defeat will grant major rewards such as new characters, new vehicles, and unsealed powers in Ultimecia Castle.

Free Roam is roughly 10% complete so far. You can fight some bosses around Balamb (step into the black clouds) and reach the western continent by train, but there is still a lot of work to be done: clearing out story-related obstacles, placing new triggers for boss encounters, and so on. Hopefully there's enough to demo the concept at least. Try it out & let me know what you think!

## Requirements

* Final Fantasy VIII - 2013 Steam release (EN)*
* No other mods installed (at least for now, if you just want to fix the music you can use [this](https://steamcommunity.com/app/39150/discussions/0/35221031741516824/))
* Windows 7 or above with [.NET Core runtime](https://docs.microsoft.com/en-us/dotnet/core/install/runtime)
* 1GB free disk space (for backups & working files)

\* Currently looking into support for the Remastered edition, as well as other regional variants of the Steam release.

## Installation & Usage

* Download the [current release](https://github.com/sleepeybunney/maelstrom/releases) (click Assets) or the latest [pre-release build](https://github.com/sleepeybunney/maelstrom/actions) (.NET Core -> Artifacts)
* Extract all files to a folder on your hard drive & run Maelstrom.exe
* Set "Game Location" by browsing to where the game is installed and selecting FF8_EN.exe
* Check the boxes for the features you want to activate & click "Go" to mod the game
* To remove any changes and restore the game to normal, uncheck all the boxes & click "Go"

## Notes

* The mod works by patching game files. You don't need to keep the Maelstrom window open after it's finished processing, and you don't ever need to run it again unless you want to re-roll.

* Saving and loading all works as it should, as long as you don't re-roll in between. It'll still load, but you'll be on the new roll instead of the one you saved. To go back to a previous playthrough, run Maelstrom with the same seed & settings as before.

* The first time you run the randomiser it will make backups (.bak) of various files in the FF8 folder tree -- don't touch those!! If they get lost or corrupted you will need to restore/reinstall the game through Steam before running Maelstrom again.

* If "Fixed Seed" is unchecked, a new seed number will be generated for each run. Checking the box allows you to use the most recent one again, or enter your own. (There are many explanations online for what an RNG "seed" is, but basically if you use the same number twice you will get the same results from the randomiser both times)

* Card locations in the spoiler file are currently incomplete. The field names are all there but the NPC descriptions are largely unhelpful.

## Planned Features

* Improved UI with more configuration options & info on what they do
* Support for Remastered & Steam EFIGS versions of the game
* More substantial rebalancing for shuffled bosses, so they are generally beatable at the point they are encountered
* Shuffled GF locations
* Random GF abilities
* Random weapon upgrade requirements
* Shuffled monster locations
* Random spells/items from bosses & monsters

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
