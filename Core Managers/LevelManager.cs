using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class LevelManager
{
    // ---------------
    // Lists of levels
    // ---------------

    // Local lists
    private static List<Level> _singlePlayerLevels;
    private static List<Level> _officialMultiplayerLevels;
    private static List<Level> _customMultiplayerLevels;

    // Tracker
    private static bool levelsLoaded = false;

    // Load all the levels from files
    public static void ReadLevelsFromFiles()
    {
        // Load single player levels
        _singlePlayerLevels = new List<Level>();
        foreach (string name in SinglePlayerLevelNames())
        {
            _singlePlayerLevels.Add(SinglePlayerLevelFromFile(name));
        }

        // Load official multiplayer levels
        _officialMultiplayerLevels = new List<Level>();
        foreach (string name in OfficialMultiplayerLevelNames())
        {
            _officialMultiplayerLevels.Add(OfficialMultiplayerLevelFromFile(name));
        }

        // Load custom multiplayer levels
        _customMultiplayerLevels = new List<Level>();
        foreach (string name in CustomMultiplayerLevelNames())
        {
            _customMultiplayerLevels.Add(CustomMultiplayerLevelFromFile(name));
        }

        levelsLoaded = true;
    }

    public static List<Level> SinglePlayerLevels()
    {
        if (!levelsLoaded) { ReadLevelsFromFiles(); }
        return _singlePlayerLevels;
    }

    public static List<Level> OfficialMultiplayerLevels(bool filtered)
    {
        if (!levelsLoaded) { ReadLevelsFromFiles(); }
        return _officialMultiplayerLevels.FindAll(item => Preferences.ArenaSelection(item.levelName) || !filtered);
    }

    public static List<Level> CustomMultiplayerLevels(bool filtered)
    {
        if (!levelsLoaded) { ReadLevelsFromFiles(); }
        return _customMultiplayerLevels.FindAll(item => Preferences.ArenaSelection(item.levelName) || !filtered);
    }

    public static List<Level> AllMultiplayerLevels(bool filtered)
    {
        if (!levelsLoaded) { ReadLevelsFromFiles(); }
        return _customMultiplayerLevels.Union(_officialMultiplayerLevels).ToList().FindAll(item => Preferences.ArenaSelection(item.levelName) || !filtered);
    }

    // --------------------
    // Lists of level names
    // --------------------

    // A list of the names of all single player levels
    public static List<string> SinglePlayerLevelNames(bool usedOnly = false)
    {
        return LevelNamesFromPath("/Levels/Single Player", usedOnly);
    }

    // A list of the names of all multiplayer player levels
    public static List<string> AllMultiplayerLevelNames(bool usedOnly = false)
    {
        List<string> officialLevelNames =  LevelNamesFromPath("/Levels/Multiplayer", usedOnly);
        List<string> customLevelNames =  LevelNamesFromPath("/Levels/Custom", usedOnly);

        return officialLevelNames.Union(customLevelNames).ToList();
    }

    // A list of the names of all official multiplayer player levels
    public static List<string> OfficialMultiplayerLevelNames(bool usedOnly = false)
    {
        return LevelNamesFromPath("/Levels/Multiplayer", usedOnly);
    }

    // A list of the names of all custom multiplayer player levels
    public static List<string> CustomMultiplayerLevelNames(bool usedOnly = false)
    {
        return LevelNamesFromPath("/Levels/Custom", usedOnly);
    }

    // Access a list of the names of levels at a given path
    private static List<string> LevelNamesFromPath(string path, bool usedOnly)
    {
        List<string> output = new List<string>();

        DirectoryInfo directoryInfo = new DirectoryInfo(Application.dataPath + path);
        FileInfo[] fileInfo = directoryInfo.GetFiles("*.txt");

        foreach (FileInfo file in fileInfo)
        {
            string name = file.Name.Substring(0, file.Name.Length - 4);

            if (!usedOnly || Preferences.ArenaSelection(name))
            {
                output.Add(name);
            }
        }

        return output;
    }


    // -------
    // Loading
    // -------

    // Load a single player level
    private static Level SinglePlayerLevelFromFile(string name)
    {        
        return LoadLevelFromFile(Application.dataPath + "/Levels/Single Player/" + name + ".txt", name, false);
    }

    // Load a official multiplayer level
    private static Level OfficialMultiplayerLevelFromFile(string name)
    {        
        return LoadLevelFromFile(Application.dataPath + "/Levels/Multiplayer/" + name + ".txt", name, false);
    }

    // Load a custom multiplayer level
    private static Level CustomMultiplayerLevelFromFile(string name)
    {        
        return LoadLevelFromFile(Application.dataPath + "/Levels/Custom/" + name + ".txt", name, true);
    }

    // Extract a level from a saved text file at the given path
    private static Level LoadLevelFromFile(string path, string name, bool custom)
    {
        // Accessing the level file:

        StreamReader reader = new StreamReader(path);

        // Create the level object:

        Level level = new Level();
        level.levelName = name;
        level.tileMatrix = new List<List<Tile>>();

        // Accessing the dimensions of the level to be loaded:

        string dimensionsString = reader.ReadLine();

        int x = dimensionsString.IndexOf("x"[0]);

        int n = int.Parse(dimensionsString.Substring(0, x - 1));
        int m = int.Parse(dimensionsString.Substring(x + 2, dimensionsString.Length - x - 2));

        // Creating empty tiles:

        for (int i = 0; i < n; i++)
        {
            List<Tile> row = new List<Tile>();

            for (int j = 0; j < m; j++)
            {
                Tile tile = new Tile();                
                row.Add(tile);
            }

            level.tileMatrix.Add(row);            
        }

        // Adding walls:

        for (int i = 0; i < n; i++)
        {
            string line = reader.ReadLine();

            for (int j = 0; j < m; j++)
            {
                Tile tile = level.tileMatrix[i][j];
                tile.closed = line[j] == "X"[0];
                level.tileMatrix[i][j] = tile;
            }
        }

        reader.ReadLine();

        // Adding surface-objects:

        for (int i = 0; i < n; i++)
        {
            string line = reader.ReadLine();

            for (int j = 0; j < m; j++)
            {
                Tile tile = level.tileMatrix[i][j];
                tile.surfaceObject = DecodeSurfaceObject(line[j].ToString());
                level.tileMatrix[i][j] = tile;
            }
        }

        reader.ReadLine();

        // Loading wiring information:
        // (wiring not yet implemented)

        for (int i = 0; i < n; i++)
        {
            string line = reader.ReadLine();

            for (int j = 0; j < m; j++)
            {
                Tile tile = level.tileMatrix[i][j];
                tile.wired = line[j] == "W"[0];
                level.tileMatrix[i][j] = tile;
            }
        }

        reader.ReadLine();

        // Is the level a custom level?
        level.custom = custom;
        
        return level;
    }


    // ------
    // Saving
    // ------

    // Save a single player level
    public static void SaveSinglePlayerLevelToFile(Level level, string name)
    {
        SaveLevelToFile(level, name, "Single Player");        
    }

    // Save a multiplayer level
    public static void SaveOfficialMultiplayerLevelToFile(Level level, string name)
    {
        SaveLevelToFile(level, name, "Multiplayer");       
    }

    // Save a multiplayer level
    public static void SaveCustomMultiplayerLevelToFile(Level level, string name)
    {
        SaveLevelToFile(level, name, "Custom");       
    }

    // Save a level to the specified folder
    private static void SaveLevelToFile(Level level, string fileName, string folderName)
    {
        List<List<Tile>> tileMatrix = level.tileMatrix;

        // Accesses the level dimensions:
        int n = level.Height;
        int m = level.Width;

        string output = "";

        string walls = "";
        string surface = "";
        string wires = "";

        output += n.ToString() + " x " + m.ToString() + "\n";

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                Tile tile = tileMatrix[i][j];
                walls += tile.closed ? "X" : " ";
                surface += EncodeSurfaceObject(tile.surfaceObject);
                wires += tile.wired ? "W" : " ";
            }

            walls += "\n";
            surface += "\n";
            wires += "\n";
        }

        walls += "\n";
        surface += "\n";

        output += walls + surface + wires;

        string path = Application.dataPath + "/Levels/" + folderName + "/" + fileName + ".txt";

        File.WriteAllText(path, output);
    }

    // --------------
    // Level Deletion
    // --------------

    public static void DeleteCustomLevel(string levelName)
    {
        File.Delete(Application.dataPath + "/Levels/Custom/" + levelName + ".txt");
    }

    // ----------------
    // Helper functions
    // ----------------

    private static string EncodeInteger(int i)
    {
        if (i <= 9)
        {
            return i.ToString();
        }
        else
        {
            // Alphabet is ASCII characters 65 - 90, so subtract 10 and add 65
            // Would be cleaner to skip digits but would need to remake levels
            // -> Convert them at some point!
            return ((char)(i + 55)).ToString();
        }
    }

    private static int DecodeInteger(string x)
    {        
        if (x == "0") return 0;
        if (x == "1") return 1;
        if (x == "2") return 2;
        if (x == "3") return 3;
        if (x == "4") return 4;
        if (x == "5") return 5;
        if (x == "6") return 6;
        if (x == "7") return 7;
        if (x == "8") return 8;
        if (x == "9") return 9;
        return x.ToCharArray()[0] - 55;    
    }

    private static string EncodeSurfaceObject(Tile.SurfaceObject surfaceObject)
    {
        int i = 0;

        switch (surfaceObject)
        {
            case Tile.SurfaceObject.none:
                i = 0;
                break;
            case Tile.SurfaceObject.crate:
                i = 1;
                break;
            case Tile.SurfaceObject.redBarrel:
                i = 2;
                break;
            case Tile.SurfaceObject.powerUp:
                i = 6;
                break;
            case Tile.SurfaceObject.playerParts:
                i = 3;
                break;
            case Tile.SurfaceObject.yellowParts:
                i = 7;
                break;
            case Tile.SurfaceObject.greenParts:
                i = 8;
                break;
            case Tile.SurfaceObject.orangeParts:
                i = 9;
                break;
            case Tile.SurfaceObject.rocketParts:
                i = 10;
                break;
            case Tile.SurfaceObject.navyParts:
                i = 11;
                break;
            case Tile.SurfaceObject.blackParts:
                i = 12;
                break;
        }

        return EncodeInteger(i);
    }

    private static Tile.SurfaceObject DecodeSurfaceObject(string x)
    {
        int i = DecodeInteger(x);
        if (i == 1) return Tile.SurfaceObject.crate;
        if (i == 2) return Tile.SurfaceObject.redBarrel;
        if (i == 3) return Tile.SurfaceObject.playerParts;
        if (i == 4) return Tile.SurfaceObject.playerParts;
        if (i == 5) return Tile.SurfaceObject.playerParts;
        if (i == 6) return Tile.SurfaceObject.powerUp;
        if (i == 7) return Tile.SurfaceObject.yellowParts;
        if (i == 8) return Tile.SurfaceObject.greenParts;
        if (i == 9) return Tile.SurfaceObject.orangeParts;
        if (i == 10) return Tile.SurfaceObject.rocketParts;
        if (i == 11) return Tile.SurfaceObject.navyParts;
        if (i == 12) return Tile.SurfaceObject.blackParts;
        return Tile.SurfaceObject.none;
    }
}

