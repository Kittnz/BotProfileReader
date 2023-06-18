using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace BotProfileReader
{
    internal class Program
    {
        static void Main()
        {
            string profilesFolderPath = System.Environment.CurrentDirectory + "\\profiles\\";
            string outputFile = System.Environment.CurrentDirectory + "\\output.sql";

            if (File.Exists(outputFile)) // Check if file exists
            {
                // Delete the file
                File.Delete(outputFile);
            }

            StringBuilder sqlBuilder = new StringBuilder();

            // Create GrinderProfile
            sqlBuilder.AppendLine($"-- Drop table if exists");
            sqlBuilder.AppendLine("DROP TABLE IF EXISTS worldbot_grinder_profiles;");
            sqlBuilder.AppendLine("-- Create table");
            sqlBuilder.AppendLine("CREATE TABLE worldbot_grinder_profiles (");
            sqlBuilder.AppendLine("    FileName VARCHAR(255),");
            sqlBuilder.AppendLine("    Name VARCHAR(255),");
            sqlBuilder.AppendLine("    Faction TEXT,");
            sqlBuilder.AppendLine("    Race TEXT,");
            sqlBuilder.AppendLine("    MapId INT,");
            sqlBuilder.AppendLine("    Hotspots TEXT,");
            sqlBuilder.AppendLine("    MinLevel INT,");
            sqlBuilder.AppendLine("    MaxLevel INT,");
            sqlBuilder.AppendLine("    MinTargetLevel INT,");
            sqlBuilder.AppendLine("    MaxTargetLevel INT,");
            sqlBuilder.AppendLine("    TargetEntry TEXT,");
            sqlBuilder.AppendLine("    TargetFactions TEXT,");
            sqlBuilder.AppendLine("    Vectors3 TEXT,");
            sqlBuilder.AppendLine("    Npc TEXT,");
            sqlBuilder.AppendLine("    BlackListRadius TEXT,");
            sqlBuilder.AppendLine("    NotLoop BOOLEAN");
            sqlBuilder.AppendLine(");");

            // Create EasyQuestProfile
            sqlBuilder.AppendLine($"-- Drop table if exists");
            sqlBuilder.AppendLine("DROP TABLE IF EXISTS worldbot_easy_quest_profiles;");
            sqlBuilder.AppendLine("-- Create table");
            sqlBuilder.AppendLine("CREATE TABLE worldbot_easy_quest_profiles (");
            sqlBuilder.AppendLine("    FileName VARCHAR(255),");
            sqlBuilder.AppendLine("    Name VARCHAR(255),");
            sqlBuilder.AppendLine("    QuestId VARCHAR(255),");
            sqlBuilder.AppendLine("    QuestType VARCHAR(255),");
            sqlBuilder.AppendLine("    QuestClassType VARCHAR(255),");
            sqlBuilder.AppendLine("    Faction TEXT,");
            sqlBuilder.AppendLine("    Race TEXT,");
            sqlBuilder.AppendLine("    MapId INT,");
            sqlBuilder.AppendLine("    HotSpots TEXT,");
            sqlBuilder.AppendLine("    EntryTarget TEXT,");
            sqlBuilder.AppendLine("    IsGrinderNotQuest TEXT,");
            sqlBuilder.AppendLine("    ObjectiveCount1 INT,");
            sqlBuilder.AppendLine("    ObjectiveCount2 INT,");
            sqlBuilder.AppendLine("    ObjectiveCount3 INT,");
            sqlBuilder.AppendLine("    ObjectiveCount4 INT,");
            sqlBuilder.AppendLine("    ObjectiveCount5 INT,");
            sqlBuilder.AppendLine("    AutoDetectObjectiveCount1 BIT,");
            sqlBuilder.AppendLine("    AutoDetectObjectiveCount2 BIT,");
            sqlBuilder.AppendLine("    AutoDetectObjectiveCount3 BIT,");
            sqlBuilder.AppendLine("    AutoDetectObjectiveCount4 BIT,");
            sqlBuilder.AppendLine("    AutoDetectObjectiveCount5 BIT,");
            sqlBuilder.AppendLine("    CanCondition TEXT,");
            sqlBuilder.AppendLine("    IsCompleteCondition TEXT,");
            sqlBuilder.AppendLine("    RepeatableQuest BIT,");
            sqlBuilder.AppendLine("    NotRequiredInQuestLog BIT,");
            sqlBuilder.AppendLine("    PickUpQuestOnItem BIT,");
            sqlBuilder.AppendLine("    PickUpQuestOnItemID INT,");
            sqlBuilder.AppendLine("    Comment TEXT,");
            sqlBuilder.AppendLine("    GossipOptionRewardItem INT,");
            sqlBuilder.AppendLine("    RequiredQuest INT,");
            sqlBuilder.AppendLine("    MaxLevel INT,");
            sqlBuilder.AppendLine("    MinLevel INT,");
            sqlBuilder.AppendLine("    WoWClass VARCHAR(255)");
            sqlBuilder.AppendLine(");");

            ProcessXmlFiles(profilesFolderPath, outputFile, sqlBuilder);

            Console.WriteLine("Parsing completed.");
        }


        static void ProcessXmlFiles(string folderPath, string outputFile, StringBuilder sqlBuilder)
        {
            string[] xmlFiles = Directory.GetFiles(folderPath, "*.xml");
            foreach (string xmlFile in xmlFiles)
            {
                Console.WriteLine($"Processing XML file: {xmlFile}");

                string outputFileName = Path.GetFileNameWithoutExtension(xmlFile) + ".sql";
                string fileFolder = Path.GetDirectoryName(xmlFile);

                //string outputPath = Path.Combine(outputFolderPath, outputFileName);

                XmlDocument xmlDoc = new XmlDocument();

                try
                {
                    Encoding encoding = GetEncoding(xmlFile);
                    using (var reader = new StreamReader(xmlFile, encoding))
                    {
                        xmlDoc.Load(reader);
                    }

                    ParseProfile(xmlDoc, outputFileName, sqlBuilder, fileFolder);

                    File.WriteAllText(outputFile, sqlBuilder.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error reading XML file: {e.Message}");
                }

                Console.WriteLine();
            }

            string[] subfolders = Directory.GetDirectories(folderPath);
            foreach (string subfolder in subfolders)
            {
                ProcessXmlFiles(subfolder, outputFile, sqlBuilder);
            }
        }

        static void ParseProfile(XmlDocument xmlDoc, string filePath, StringBuilder sqlBuilder, string fileFolder)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            fileName = FixString(fileName);

            //string fileFolder = Path.GetDirectoryName(filePath);

            //StringBuilder sqlBuilder = new StringBuilder();
            Console.WriteLine(xmlDoc.DocumentElement.Name);
            Console.WriteLine("//----------------------------------------");

            // Check if it's a GrinderProfile or EasyQuestProfile based on the root element
            if (xmlDoc.DocumentElement.Name == "GrinderProfile")
            {
                XmlNodeList zoneNodes = xmlDoc.SelectNodes("//GrinderProfile/GrinderZones/GrinderZone");
                foreach (XmlNode zoneNode in zoneNodes)
                {
                    string name = zoneNode.SelectSingleNode("Name")?.InnerText;
                    bool hotspots = Convert.ToBoolean(zoneNode.SelectSingleNode("Hotspots")?.InnerText);
                    int minLevel = Convert.ToInt32(zoneNode.SelectSingleNode("MinLevel")?.InnerText);
                    int maxLevel = Convert.ToInt32(zoneNode.SelectSingleNode("MaxLevel")?.InnerText);
                    int minTargetLevel = Convert.ToInt32(zoneNode.SelectSingleNode("MinTargetLevel")?.InnerText);
                    int maxTargetLevel = Convert.ToInt32(zoneNode.SelectSingleNode("MaxTargetLevel")?.InnerText);
                    string targetEntry = GetInnerTextFromNodeList(zoneNode.SelectNodes("TargetEntry/int"));
                    string targetFactions = GetInnerTextFromNodeList(zoneNode.SelectNodes("TargetFactions/*"));
                    string vectors3 = GetVectors3Data(zoneNode.SelectNodes("Vectors3/Vector3"));
                    string npc = zoneNode.SelectSingleNode("Npc")?.InnerText;
                    string blackListRadius = zoneNode.SelectSingleNode("BlackListRadius")?.InnerText;
                    bool notLoop = Convert.ToBoolean(zoneNode.SelectSingleNode("NotLoop")?.InnerText);

                    // Generate the INSERT statement for each GrinderZone
                    sqlBuilder.AppendLine($"INSERT INTO worldbot_grinder_profiles (FileName, Name, Faction, Race, MapId, Hotspots, MinLevel, MaxLevel, MinTargetLevel, MaxTargetLevel, TargetEntry, TargetFactions, Vectors3, Npc, BlackListRadius, NotLoop) " +
                        $"VALUES ('{fileName}', '{name}', 0,  0,  0, {hotspots}, {minLevel}, {maxLevel}, {minTargetLevel}, {maxTargetLevel}, '{targetEntry}', '{targetFactions}', '{vectors3}', '{npc}', '{blackListRadius}', {notLoop});");
                }
            }
            else if (xmlDoc.DocumentElement.Name == "EasyQuestProfile")
            {
                XmlNodeList npcNodes = xmlDoc.SelectNodes("//EasyQuestProfile/Npc/Npc");
                string ContinentId = "";
                foreach (XmlNode npcNode in npcNodes)
                {
                    ContinentId = npcNode.SelectSingleNode("ContinentId")?.InnerText;
                }

                XmlNodeList questNodes = xmlDoc.SelectNodes("//EasyQuestProfile/EasyQuests/EasyQuest");
                foreach (XmlNode questNode in questNodes)
                {
                    string name = questNode.SelectSingleNode("Name")?.InnerText;
                    name = FixString(name);

                    string questId = questNode.SelectSingleNode("QuestId")?.InnerText;
                    string questType = questNode.SelectSingleNode("QuestType")?.InnerText;

                    XmlNode questClassNode = questNode.SelectSingleNode("QuestClass");
                    string questClassType = questClassNode?.Attributes["xsi:type"]?.InnerText;

                    XmlNodeList hotSpotNodes = questClassNode?.SelectNodes("HotSpots/Vector3");
                    StringBuilder hotSpotsBuilder = new StringBuilder();
                    if (hotSpotNodes != null)
                    {
                        foreach (XmlNode hotSpotNode in hotSpotNodes)
                        {
                            string x = hotSpotNode.Attributes["X"]?.Value;
                            string y = hotSpotNode.Attributes["Y"]?.Value;
                            string z = hotSpotNode.Attributes["Z"]?.Value;
                            hotSpotsBuilder.Append($"({x}, {y}, {z}), ");
                        }
                    }
                    string hotSpots = hotSpotsBuilder.ToString().TrimEnd(',', ' ');

                    XmlNodeList entryTargetNodes = questClassNode?.SelectNodes("EntryTarget/int");
                    StringBuilder entryTargetBuilder = new StringBuilder();
                    if (entryTargetNodes != null)
                    {
                        foreach (XmlNode entryTargetNode in entryTargetNodes)
                        {
                            string currentEntryTarget = entryTargetNode.InnerText;
                            entryTargetBuilder.Append($"{currentEntryTarget}, ");
                        }
                    }
                    string entryTargetResult = entryTargetBuilder.ToString().TrimEnd(',', ' ');

                    string isGrinderNotQuest = questClassNode?.SelectSingleNode("IsGrinderNotQuest")?.InnerText;

                    if (isGrinderNotQuest != "true" || isGrinderNotQuest != "false")
                        isGrinderNotQuest = "false";

                    string objectiveCount1 = questNode.SelectSingleNode("ObjectiveCount1")?.InnerText;
                    string objectiveCount2 = questNode.SelectSingleNode("ObjectiveCount2")?.InnerText;
                    string objectiveCount3 = questNode.SelectSingleNode("ObjectiveCount3")?.InnerText;
                    string objectiveCount4 = questNode.SelectSingleNode("ObjectiveCount4")?.InnerText;
                    string objectiveCount5 = questNode.SelectSingleNode("ObjectiveCount5")?.InnerText;

                    string autoDetectObjectiveCount1 = questNode.SelectSingleNode("AutoDetectObjectiveCount1")?.InnerText;
                    string autoDetectObjectiveCount2 = questNode.SelectSingleNode("AutoDetectObjectiveCount2")?.InnerText;
                    string autoDetectObjectiveCount3 = questNode.SelectSingleNode("AutoDetectObjectiveCount3")?.InnerText;
                    string autoDetectObjectiveCount4 = questNode.SelectSingleNode("AutoDetectObjectiveCount4")?.InnerText;
                    string autoDetectObjectiveCount5 = questNode.SelectSingleNode("AutoDetectObjectiveCount5")?.InnerText;

                    string canCondition = questNode.SelectSingleNode("CanCondition")?.InnerText;
                    canCondition = FixString(canCondition);

                    string isCompleteCondition = questNode.SelectSingleNode("IsCompleteCondition")?.InnerText;

                    string repeatableQuest = questNode.SelectSingleNode("RepeatableQuest")?.InnerText;
                    string notRequiredInQuestLog = questNode.SelectSingleNode("NotRequiredInQuestLog")?.InnerText;
                    string pickUpQuestOnItem = questNode.SelectSingleNode("PickUpQuestOnItem")?.InnerText;
                    string pickUpQuestOnItemID = questNode.SelectSingleNode("PickUpQuestOnItemID")?.InnerText;

                    string comment = questNode.SelectSingleNode("Comment")?.InnerText;
                    string gossipOptionRewardItem = questNode.SelectSingleNode("GossipOptionRewardItem")?.InnerText;
                    string requiredQuest = questNode.SelectSingleNode("RequiredQuest")?.InnerText;
                    string maxLevel = questNode.SelectSingleNode("MaxLevel")?.InnerText;
                    string minLevel = questNode.SelectSingleNode("MinLevel")?.InnerText;
                    string wowClass = questNode.SelectSingleNode("WoWClass")?.InnerText;

                    string faction = "";
                    if (Regex.IsMatch(fileFolder, "horde", RegexOptions.IgnoreCase))
                        faction = "horde";

                    if (Regex.IsMatch(fileFolder, "alliance", RegexOptions.IgnoreCase))
                        faction = "alliance";

                    string race = "";

                    if (faction == "horde")
                        race = "orc";

                    if (faction == "alliance")
                        race = "human";

                    if (Regex.IsMatch(fileName, "human", RegexOptions.IgnoreCase))
                        race = "human";

                    int mapid = 0;
                    if (ContinentId == "Azeroth")
                        mapid = 0;
                    if (ContinentId == "Kalimdor")
                        mapid = 1;
                    if (ContinentId == "")
                        mapid = 2;

                    if (Regex.IsMatch(fileName, "undead", RegexOptions.IgnoreCase))
                    {
                        mapid = 0;
                        race = "undead";
                    }

                    if (Regex.IsMatch(fileName, "tauren", RegexOptions.IgnoreCase))
                    {
                        mapid = 1;
                        race = "tauren";
                    }

                    if (mapid == 2)
                    {
                        if (Regex.IsMatch(fileName, "horde", RegexOptions.IgnoreCase))
                            mapid = 1;
                    }

                    // Generate the INSERT statement for each EasyQuest
                    sqlBuilder.AppendLine($"INSERT INTO worldbot_easy_quest_profiles (FileName, Name, QuestId, QuestType, QuestClassType, Faction, Race, MapId, HotSpots, EntryTarget, IsGrinderNotQuest, " +
                    $"ObjectiveCount1, ObjectiveCount2, ObjectiveCount3, ObjectiveCount4, ObjectiveCount5, " +
                    $"AutoDetectObjectiveCount1, AutoDetectObjectiveCount2, AutoDetectObjectiveCount3, AutoDetectObjectiveCount4, AutoDetectObjectiveCount5, " +
                    $"CanCondition, IsCompleteCondition, RepeatableQuest, NotRequiredInQuestLog, PickUpQuestOnItem, PickUpQuestOnItemID, " +
                    $"Comment, GossipOptionRewardItem, RequiredQuest, MaxLevel, MinLevel, WoWClass) " +
                    $"VALUES ('{fileName}', '{name}', '{questId}', '{questType}', '{questClassType}', '{faction}', '{race}', {mapid}, '{hotSpots}', '{entryTargetResult}', '{isGrinderNotQuest}', " +
                    $"{objectiveCount1}, {objectiveCount2}, {objectiveCount3}, {objectiveCount4}, {objectiveCount5}, " +
                    $"{autoDetectObjectiveCount1}, {autoDetectObjectiveCount2}, {autoDetectObjectiveCount3}, {autoDetectObjectiveCount4}, {autoDetectObjectiveCount5}, " +
                    $"'{canCondition}', '{isCompleteCondition}', {repeatableQuest}, {notRequiredInQuestLog}, {pickUpQuestOnItem}, {pickUpQuestOnItemID}, " +
                    $"'{comment}', {gossipOptionRewardItem}, {requiredQuest}, {maxLevel}, {minLevel}, '{wowClass}');");
                }
            }
            else
            {
                Console.WriteLine($"Unsupported profile type: {fileName}");
            }

            sqlBuilder.AppendLine("");
            //return sqlBuilder.ToString();
        }

        static string GetInnerTextFromNodeList(XmlNodeList nodeList)
        {
            StringBuilder sb = new StringBuilder();
            foreach (XmlNode node in nodeList)
            {
                sb.Append(node.InnerText).Append(",");
            }
            return sb.ToString().TrimEnd(',');
        }

        static string GetVectors3Data(XmlNodeList vectorNodes)
        {
            StringBuilder sb = new StringBuilder();
            foreach (XmlNode vectorNode in vectorNodes)
            {
                string x = vectorNode.Attributes["X"]?.Value;
                string y = vectorNode.Attributes["Y"]?.Value;
                string z = vectorNode.Attributes["Z"]?.Value;
                sb.Append($"({x},{y},{z})").Append(",");
            }
            return sb.ToString().TrimEnd(',');
        }

        // Auto-detect the encoding of the XML file
        static Encoding GetEncoding(string filePath)
        {
            using (var reader = new StreamReader(filePath, true))
            {
                reader.Peek(); // Read the first character to detect encoding
                return reader.CurrentEncoding;
            }
        }

        static void WriteToSqlFile(string filePath, string sql)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    writer.WriteLine(sql);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error writing to SQL file: {e.Message}");
            }
        }

        public static string FixString(string str)
        {
            str = str.Replace("'", "''");
            /*string str2 = new string((from c in str where char.IsWhiteSpace(c) || char.IsLetterOrDigit(c) select c).ToArray());
            string str3 = str2.TrimStart();
            string str4 = str3.TrimEnd();
            return str4;*/
            return str;
        }
    }
}