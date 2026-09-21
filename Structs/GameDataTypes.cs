using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using static ImpostersOrdeal.ExternalJsonStructs;
using static ImpostersOrdeal.GlobalData;

namespace ImpostersOrdeal
{
    /// <summary>
    ///  Container for game-related data types for use in the application.
    /// </summary>
    public static class GameDataTypes
    {
        public class EvScript
        {
            public string mName;
            public List<Script> scripts;
            public List<string> strList;
        }

        public class EvFlowGraph
        {
            // label -> labels this label points to
            public Dictionary<string, HashSet<string>> Forward { get; } = new();

            // label -> labels that point to this label
            public Dictionary<string, HashSet<string>> Backward { get; } = new();

            public void AddLabel(string label)
            {
                if (string.IsNullOrEmpty(label))
                    return;

                if (!Forward.ContainsKey(label))
                    Forward[label] = new HashSet<string>();

                if (!Backward.ContainsKey(label))
                    Backward[label] = new HashSet<string>();
            }

            public void AddConnection(string from, string to)
            {
                if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to))
                    return;

                AddLabel(from);
                AddLabel(to);

                Forward[from].Add(to);
                Backward[to].Add(from);
            }

            /// <summary>
            /// Gets every message/script label belonging to the same flow
            /// as the supplied starting label.
            ///
            /// Traverses both forward and backward, allowing the search to
            /// begin anywhere in the flow.
            /// </summary>
            public List<string> GetFlow(string startLabel)
            {
                if (string.IsNullOrEmpty(startLabel))
                    return new List<string>();

                if (!Forward.ContainsKey(startLabel))
                    return new List<string>();

                HashSet<string> visited = new();
                Queue<string> queue = new();

                visited.Add(startLabel);
                queue.Enqueue(startLabel);

                while (queue.Count > 0)
                {
                    string current = queue.Dequeue();

                    // Follow forward connections.
                    foreach (string next in Forward[current])
                    {
                        if (visited.Add(next))
                            queue.Enqueue(next);
                    }

                    // Follow backward connections.
                    foreach (string previous in Backward[current])
                    {
                        if (visited.Add(previous))
                            queue.Enqueue(previous);
                    }
                }

                return visited.ToList();
            }
        }

        public class Script
        {
            public string evLabel;
            public List<Command> commands;
        }

        public class Command
        {
            public int cmdType;
            public List<Argument> args;
        }

        public class Argument
        {
            public int argType;
            public float data;
        }

        public class MapWarpAsset
        {
            public string mName;
            public List<MapWarp> mapWarps;
            public List<int> zoneIDs;
        }

        public class MapWarp
        {
            public int groupId;
            public int destWarpZone;
            public int destWarpIndex;
            public int inputDir;
            public int flagIndex;
            public int scriptLabel;
            public int exitLabel;
            public string connectionName;

            //Readonly
            public int currentWarpIndex;
            public MapWarpAsset destination;
        }

        public class PickupItem
        {
            public ushort itemID;
            public List<byte> ratios;
        }

        public class ShopTables
        {
            public List<MartItem> martItems;
            public List<FixedShopItem> fixedShopItems;
            public List<BpShopItem> bpShopItems;
        }

        public class MartItem
        {
            public ushort itemID;
            public int badgeNum;
            public int zoneID;
        }

        public class FixedShopItem
        {
            public ushort itemID;
            public int shopID;
        }

        public class BpShopItem
        {
            public ushort itemID;
            public int npcID;
        }

        public class Trainer : INamedEntity
        {
            public int trainerTypeID;
            public byte colorID;
            public byte fightType;
            public int arenaID;
            public int effectID;
            public byte gold;
            public ushort useItem1;
            public ushort useItem2;
            public ushort useItem3;
            public ushort useItem4;
            public byte hpRecoverFlag;
            public ushort giftItem;
            public string nameLabel;
            public uint aiBit;
            public List<TrainerPokemon> trainerPokemon;

            //Readonly
            public int trainerID;
            public string name;

            public Trainer() { }

            public Trainer(Trainer t)
            {
                SetAll(t);
            }

            public void SetAll(Trainer t)
            {
                trainerTypeID = t.trainerTypeID;
                colorID = t.colorID;
                fightType = t.fightType;
                arenaID = t.arenaID;
                effectID = t.effectID;
                gold = t.gold;
                useItem1 = t.useItem1;
                useItem2 = t.useItem2;
                useItem3 = t.useItem3;
                useItem4 = t.useItem4;
                hpRecoverFlag = t.hpRecoverFlag;
                giftItem = t.giftItem;
                nameLabel = t.nameLabel;
                aiBit = t.aiBit;
                trainerPokemon = new();
                foreach (TrainerPokemon tp in t.trainerPokemon)
                    trainerPokemon.Add(new(tp));
                trainerID = t.trainerID;
                name = t.name;
            }

            public List<int> GetItems()
            {
                List<int> items = new();
                if (useItem1 > 0)
                    items.Add(useItem1);
                if (useItem2 > 0)
                    items.Add(useItem2);
                if (useItem3 > 0)
                    items.Add(useItem3);
                if (useItem4 > 0)
                    items.Add(useItem4);
                return items;
            }

            public void SetItems(List<int> items)
            {
                useItem1 = (ushort)(items.Count > 0 ? items[0] : 0);
                useItem2 = (ushort)(items.Count > 1 ? items[1] : 0);
                useItem3 = (ushort)(items.Count > 2 ? items[2] : 0);
                useItem4 = (ushort)(items.Count > 3 ? items[3] : 0);
            }

            public void SetItemFlag()
            {
                aiBit |= 1 << 5;
            }

            public int GetTypeTheme()
            {
                return trainerTypeID switch
                {
                    80 => 1,
                    69 => 4,
                    65 => 5,
                    68 => 6,
                    81 => 7,
                    67 => 8,
                    70 => 9,
                    79 => 10,
                    78 => 11,
                    83 => 12,
                    71 => 13,
                    82 => 14,
                    _ => -1,
                };
            }

            public double GetAvgLevel()
            {
                if (trainerPokemon.Count == 0)
                    return 0;
                return trainerPokemon.Select(p => (int)p.level).Average();
            }

            public bool[] GetAIFlags()
            {
                bool[] flags = new bool[32];
                for (int i = 0; i < 32; i++)
                    flags[i] = (aiBit & ((uint)1 << i)) != 0;
                return flags;
            }

            public void SetAIFlags(bool[] flagArray)
            {
                aiBit = 0;
                for (int i = 0; i < 32; i++)
                    aiBit |= flagArray[i] ? (uint)1 << i : 0;
            }

            public int GetID()
            {
                return trainerID;
            }

            public string GetName()
            {
                return name;
            }

            public bool IsValid()
            {
                return true;
            }
        }

        public class TrainerPokemon
        {
            public ushort dexID;
            public ushort formID;
            public byte isRare;
            public byte level;
            public byte sex;
            public byte natureID;
            public ushort abilityID;
            public ushort moveID1;
            public ushort moveID2;
            public ushort moveID3;
            public ushort moveID4;
            public ushort itemID;
            public byte ballID;
            public int seal;
            public byte hpIV;
            public byte atkIV;
            public byte defIV;
            public byte spAtkIV;
            public byte spDefIV;
            public byte spdIV;
            public byte hpEV;
            public byte atkEV;
            public byte defEV;
            public byte spAtkEV;
            public byte spDefEV;
            public byte spdEV;

            public TrainerPokemon() { }

            public TrainerPokemon(TrainerPokemon tp)
            {
                dexID = tp.dexID;
                formID = tp.formID;
                isRare = tp.isRare;
                level = tp.level;
                sex = tp.sex;
                natureID = tp.natureID;
                abilityID = tp.abilityID;
                moveID1 = tp.moveID1;
                moveID2 = tp.moveID2;
                moveID3 = tp.moveID3;
                moveID4 = tp.moveID4;
                itemID = tp.itemID;
                ballID = tp.ballID;
                seal = tp.seal;
                hpIV = tp.hpIV;
                atkIV = tp.atkIV;
                defIV = tp.defIV;
                spdIV = tp.spdIV;
                spAtkIV = tp.spAtkIV;
                spDefIV = tp.spDefIV;
                hpEV = tp.hpEV;
                atkEV = tp.atkEV;
                defEV = tp.defEV;
                spdEV = tp.spdEV;
                spAtkEV = tp.spAtkEV;
                spDefEV = tp.spDefEV;
            }

            public List<ushort> GetMoves()
            {
                List<ushort> moves = new();
                if (moveID1 > 0)
                    moves.Add(moveID1);
                if (moveID2 > 0)
                    moves.Add(moveID2);
                if (moveID3 > 0)
                    moves.Add(moveID3);
                if (moveID4 > 0)
                    moves.Add(moveID4);
                return moves;
            }

            public void SetMoves(List<ushort> moves)
            {
                moveID1 = (ushort)(moves.Count > 0 ? moves[0] : 0);
                moveID2 = (ushort)(moves.Count > 1 ? moves[1] : 0);
                moveID3 = (ushort)(moves.Count > 2 ? moves[2] : 0);
                moveID4 = (ushort)(moves.Count > 3 ? moves[3] : 0);
            }

            public int[] GetIVs()
            {
                return new int[]
                {
                    hpIV,
                    atkIV,
                    defIV,
                    spAtkIV,
                    spDefIV,
                    spdIV
                };
            }

            public int[] GetEVs()
            {
                return new int[]
                {
                    hpEV,
                    atkEV,
                    defEV,
                    spAtkEV,
                    spDefEV,
                    spdEV
                };
            }

            public void SetEVs(int[] evs)
            {
                hpEV = (byte)evs[0];
                atkEV = (byte)evs[1];
                defEV = (byte)evs[2];
                spAtkEV = (byte)evs[3];
                spDefEV = (byte)evs[4];
                spdEV = (byte)evs[5];
            }
        }

        public class BattleTowerTrainerPokemon
        {
            public uint pokemonID;
            public int dexID;
            public uint formID;
            public byte isRare;
            public byte level;
            public byte sex;
            public int natureID;
            public int abilityID;
            public int moveID1;
            public int moveID2;
            public int moveID3;
            public int moveID4;
            public ushort itemID;
            public byte ballID;
            public int seal;
            public byte hpIV;
            public byte atkIV;
            public byte defIV;
            public byte spAtkIV;
            public byte spDefIV;
            public byte spdIV;
            public byte hpEV;
            public byte atkEV;
            public byte defEV;
            public byte spAtkEV;
            public byte spDefEV;
            public byte spdEV;
            

            public BattleTowerTrainerPokemon() { }

            public BattleTowerTrainerPokemon(BattleTowerTrainerPokemon tp)
            {
                dexID = tp.dexID;
                formID = tp.formID;
                isRare = tp.isRare;
                level = tp.level;
                sex = tp.sex;
                natureID = tp.natureID;
                abilityID = tp.abilityID;
                moveID1 = tp.moveID1;
                moveID2 = tp.moveID2;
                moveID3 = tp.moveID3;
                moveID4 = tp.moveID4;
                itemID = tp.itemID;
                ballID = tp.ballID;
                seal = tp.seal;
                hpIV = tp.hpIV;
                atkIV = tp.atkIV;
                defIV = tp.defIV;
                spdIV = tp.spdIV;
                spAtkIV = tp.spAtkIV;
                spDefIV = tp.spDefIV;
                hpEV = tp.hpEV;
                atkEV = tp.atkEV;
                defEV = tp.defEV;
                spdEV = tp.spdEV;
                spAtkEV = tp.spAtkEV;
                spDefEV = tp.spDefEV;
               
            }

            public List<int> GetMoves()
            {
                List<int> moves = new();
                if (moveID1 > 0)
                    moves.Add(moveID1);
                if (moveID2 > 0)
                    moves.Add(moveID2);
                if (moveID3 > 0)
                    moves.Add(moveID3);
                if (moveID4 > 0)
                    moves.Add(moveID4);
                return moves;
            }

            public void SetMoves(List<ushort> moves)
            {
                moveID1 = (ushort)(moves.Count > 0 ? moves[0] : 0);
                moveID2 = (ushort)(moves.Count > 1 ? moves[1] : 0);
                moveID3 = (ushort)(moves.Count > 2 ? moves[2] : 0);
                moveID4 = (ushort)(moves.Count > 3 ? moves[3] : 0);
            }

            public int[] GetIVs()
            {
                return new int[]
                {
                    hpIV,
                    atkIV,
                    defIV,
                    spAtkIV,
                    spDefIV,
                    spdIV
                };
            }

            public int[] GetEVs()
            {
                return new int[]
                {
                    hpEV,
                    atkEV,
                    defEV,
                    spAtkEV,
                    spDefEV,
                    spdEV
                };
            }

            public void SetEVs(int[] evs)
            {
                hpEV = (byte)evs[0];
                atkEV = (byte)evs[1];
                defEV = (byte)evs[2];
                spAtkEV = (byte)evs[3];
                spDefEV = (byte)evs[4];
                spdEV = (byte)evs[5];
            }
            public uint GetID()
            {
                return pokemonID;
            }

            public int GetName()
            {   
                return dexID;
            }
        }

        public class BattleTowerTrainer : INamedEntity
        {
            public uint trainerID2;
            public int trainerTypeID;
            public int trainerTypeID2;
            public uint battleTowerPokemonID1;
            public uint battleTowerPokemonID2;
            public uint battleTowerPokemonID3;
            public uint battleTowerPokemonID4 = 0;
            public string battleBGM;
            public string winBGM;
            public bool isDouble;
            public string nameLabel;
            public string nameLabel2;
            public bool wasCalled = false;

            //Readonly
            public int trainerID;
            public string name;
            public string name2;
            public BattleTowerTrainer() { }

            public BattleTowerTrainer(BattleTowerTrainer t)
            {
                SetAll(t);
            }

            public void SetAll(BattleTowerTrainer t)
            {
                trainerID2 = t.trainerID2;
                trainerTypeID = t.trainerTypeID;
                if (t.trainerTypeID2 != -1)
                {
                    trainerTypeID2 = t.trainerTypeID2;
                }
                battleTowerPokemonID1 = t.battleTowerPokemonID1;    
                battleTowerPokemonID2 = t.battleTowerPokemonID2;    
                battleTowerPokemonID3 = t.battleTowerPokemonID3;  
                if(t.battleTowerPokemonID4 != 0)
                {
                    battleTowerPokemonID4 = t.battleTowerPokemonID4;
                }
                battleBGM = t.battleBGM;
                winBGM = t.winBGM;
                name = t.name;
              //  name2 = t.name2;
            }

            public int GetID()
            {
                return trainerTypeID;
            }

            public string GetInternalID()
            {
                return trainerID2.ToString();
            }

            public string GetName()
            {
                if (name2 == null)
                {
                }
                else if(wasCalled == false)
                {
                    name = name + " & " + name2;
                    wasCalled = true;
                }
                return name.ToString();
            }

            public bool IsValid()
            {
                return true;
            }
        }

        public class TrainerType : INamedEntity
        {
            public int trainerTypeID;
            public string label;
            public string name;

            public int GetID()
            {
                return trainerTypeID;
            }

            public string GetName()
            {
                return name;
            }

            public bool IsValid()
            {
                return label != "";
            }
        }

        public class TradePokemon
        {
            public int target;
            public string nameLabel;
            public int trainerId;
            public int monsNo;
            public string nicknameLabel;
            public int level;
            public int natureID; // aka seikaku
            public int abilityID; // aka tokusei
            public int itemNo;
            public int rand;
            public int sex;
            public int language;
            public List<int> moves = new();
        }

        public class EncounterTableFile
        {
            public string mName;
            public List<EncounterTable> encounterTables;
            public List<int> trophyGardenMons;
            public List<HoneyTreeEncounter> honeyTreeEnconters;
            public List<int> safariMons;
        }

        public class EncounterTable
        {
            public ZoneID zoneID;
            public int encRateGround;
            public List<Encounter> groundMons;
            public List<Encounter> tairyo;
            public List<Encounter> day;
            public List<Encounter> night;
            public List<Encounter> swayGrass;
            public int formProb;
            public int unownTable;
            public List<Encounter> gbaRuby;
            public List<Encounter> gbaSapphire;
            public List<Encounter> gbaEmerald;
            public List<Encounter> gbaFire;
            public List<Encounter> gbaLeaf;
            public int encRateWater;
            public List<Encounter> waterMons;
            public int encRateOldRod;
            public List<Encounter> oldRodMons;
            public int encRateGoodRod;
            public List<Encounter> goodRodMons;
            public int encRateSuperRod;
            public List<Encounter> superRodMons;

            public List<List<Encounter>> GetAllTables()
            {
                return new List<List<Encounter>>()
                {
                    groundMons, tairyo, day, night, swayGrass,
                    gbaRuby, gbaSapphire, gbaEmerald, gbaFire, gbaLeaf,
                    waterMons, oldRodMons, goodRodMons, superRodMons
                };
            }

            public double GetAvgLevel()
            {
                return GetAllTables()
                    .Take(5)
                    .SelectMany(l => l)
                    .Where(e => e.dexID != 0)
                    .Select(e => e.GetAvgLevel())
                    .DefaultIfEmpty()
                    .Average();
            }
        }

        public class Encounter
        {
            public int maxLv;
            public int minLv;
            public int dexID;

            public double GetAvgLevel()
            {
                return (minLv + maxLv) / 2.0;
            }
        }

        public class HoneyTreeEncounter
        {
            public int rate;
            public int normalDexID;
            public int rareDexID;
            public int superRareDexID;
        }

        public class MessageFileSet
        {
            public Language langID;
            public List<MessageFile> messageFiles;

            public List<LabelData> GetStrings()
            {
                List<LabelData> strings = new();
                for (int i = 0; i < messageFiles.Count; i++)
                    strings.AddRange(messageFiles[i].GetStrings());
                return strings;
            }

            public void SetStrings(List<LabelData> strings)
            {
                for (int i = messageFiles.Count - 1; i >= 0; i--)
                    messageFiles[i].SetStrings(strings);
            }
        }

        public class MessageFile
        {
            public string mName;
            public Language langID;
            public byte isKanji;
            public List<LabelData> labelDatas;

            public List<LabelData> GetStrings()
            {
                List<LabelData> strings = new();
                for (int i = 0; i < labelDatas.Count; i++)
                    if (labelDatas[i].IsValidString())
                        strings.Add(labelDatas[i]);
                return strings;
            }

            public void SetStrings(List<LabelData> strings)
            {
                for (int i = labelDatas.Count - 1; i >= 0; i--)
                {
                    if (labelDatas[i].IsValidString())
                    {
                        labelDatas[i].wordDatas = new();

                        foreach (WordData wd in strings[^1].wordDatas)
                            labelDatas[i].wordDatas.Add(
                                (WordData)wd.Clone()
                            );

                        labelDatas[i].tagDatas = new();

                        foreach (TagData td in strings[^1].tagDatas)
                            labelDatas[i].tagDatas.Add(
                                (TagData)td.Clone()
                            );

                        strings.RemoveAt(strings.Count - 1);
                    }
                }
            }
        }
        public class TagData : ICloneable
        {
            public int tagIndex;
            public int groupID;
            public int tagID;
            public int tagPatternID;
            public int forceArticle;
            public int tagParameter;
            public List<string> tagWordArray = new();
            public int forceGrmID;

            public object Clone()
            {
                TagData td = (TagData)MemberwiseClone();
                td.tagWordArray = new();
                td.tagWordArray.AddRange(tagWordArray);
                return td;
            }
        }

        public class LabelData : ICloneable
        {
            public int labelIndex;
            public int arrayIndex;
            public string labelName;
            public int styleIndex;
            public int colorIndex;
            public int fontSize;
            public int maxWidth;
            public int controlID;
            public List<int> attributeValues;
            public List<TagData> tagDatas;
            public List<WordData> wordDatas;

            public object Clone()
            {
                LabelData ld = (LabelData)MemberwiseClone();
                ld.attributeValues = new();
                ld.attributeValues.AddRange(attributeValues);
                ld.tagDatas = new();
                foreach (TagData td in tagDatas)
                    ld.tagDatas.Add((TagData)td.Clone());
                ld.wordDatas = new();
                foreach (WordData wd in wordDatas)
                    ld.wordDatas.Add((WordData)wd.Clone());

                return ld;
            }

            public string GetString()
            {
                string str = "";
                for (int i = 0; i < wordDatas.Count; i++)
                    str += wordDatas[i].str + wordDatas[i].GetEndChar();
                return str;
            }

            public bool IsValidString()
            {
                if (GetString().Length < 1)
                    return false;

                for (int i = 0; i < wordDatas.Count; i++)
                {
                    WordData wd = wordDatas[i];

                    if (wd.IsWordTag())
                        return false;

                    if (wd.eventID == (int)MsbtEnums.MsgEventID.CallBack)
                        return false;
                }

                return true;
            }

            public bool IsDialogString()
            {
                for (int i = 0; i < wordDatas.Count; i++)
                    if (wordDatas[i].eventID == 3)
                        return true;
                return false;
            }

            public string GetMacroString()
            {
                string s = "";

                foreach (WordData wd in wordDatas)
                {
                    if (wd.IsWordTag())
                    {
                        if (wd.tagIndex < 0 || wd.tagIndex >= tagDatas.Count)
                        {
                            throw new InvalidOperationException(
                                $"WordData contains invalid tagIndex {wd.tagIndex}"
                            );
                        }

                        s += wd.str + wd.GetMacro(tagDatas[wd.tagIndex]);
                    }
                    else
                    {
                        s += wd.str + wd.GetMacro();
                    }
                }

                return s;
            }

            public void SetMacroString(string s)
            {
                tagDatas.Clear();

                List<WordData> newWordDatas = new();

                while (s.Length > 0)
                {
                    // ============================================================
                    // HTML-style tag
                    // ============================================================

                    Match htmlTagMatch = Regex.Match(
                        s,
                        @"\A<[^>]+>"
                    );

                    if (htmlTagMatch.Success)
                    {
                        string tag = htmlTagMatch.Value;

                        string tagName = tag
                            .Trim('<', '>')
                            .TrimStart('/')
                            .Split(' ', '=', '\t')[0];

                        int patternID;

                        if (tagName.StartsWith(
                                "color",
                                StringComparison.OrdinalIgnoreCase))
                        {
                            patternID = (int)MsbtEnums.WordDataPatternID.ColorTag;
                        }
                        else if (tagName.StartsWith(
                                    "size",
                                    StringComparison.OrdinalIgnoreCase))
                        {
                            patternID = (int)MsbtEnums.WordDataPatternID.SizeTag;
                        }
                        else if (tagName.StartsWith(
                                    "font",
                                    StringComparison.OrdinalIgnoreCase))
                        {
                            patternID = (int)MsbtEnums.WordDataPatternID.FontTag;
                        }
                        else
                        {
                            patternID = (int)MsbtEnums.WordDataPatternID.CtrlTag;
                        }

                        newWordDatas.Add(new WordData
                        {
                            patternID = patternID,
                            eventID = (int)MsbtEnums.MsgEventID.NONE,
                            tagIndex = -1,
                            tagValue = 0,
                            str = tag,
                            strWidth = -1f
                        });

                        s = s[tag.Length..];

                        continue;
                    }

                    // ============================================================
                    // MSBT WordTag
                    //
                    // {tagIndex}
                    // {tagIndex,groupID,tagID}
                    // {tagIndex,groupID,tagID,tagParameter}
                    // ============================================================

                    if (s[0] == '{')
                    {
                        int closeIndex = s.IndexOf('}');

                        if (closeIndex < 0)
                        {
                            throw new ArgumentException(
                                "Unterminated tag: " + s
                            );
                        }

                        string tagContents = s.Substring(
                            1,
                            closeIndex - 1
                        );

                        string[] args = tagContents
                            .Replace(" ", "")
                            .Split(',');

                        if (args.Length == 0 ||
                            !int.TryParse(args[0], out int tagIndex))
                        {
                            throw new ArgumentException(
                                "Invalid tag: {" + tagContents + "}"
                            );
                        }

                        int groupID = args.Length > 1
                            ? int.Parse(args[1])
                            : (int)MsbtEnums.GroupTagID.Name;

                        int tagID = args.Length > 2
                            ? int.Parse(args[2])
                            : (int)MsbtEnums.NameTagID.Default;

                        int tagParam = args.Length > 3
                            ? int.Parse(args[3])
                            : 0;

                        int tagPatternID =
                            groupID == (int)MsbtEnums.GroupTagID.Digit
                                ? (int)MsbtEnums.TagPatternID.Digit
                                : (int)MsbtEnums.TagPatternID.Word;

                        // This is the index into LabelData.tagDatas.
                        int tagDataIndex = tagDatas.Count;

                        tagDatas.Add(new TagData
                        {
                            tagIndex = tagIndex,
                            groupID = groupID,
                            tagID = tagID,
                            tagPatternID = tagPatternID,
                            forceArticle = 0,
                            tagParameter = tagParam,
                            tagWordArray = new(),
                            forceGrmID = (int)MsbtEnums.ForceGrmTagID.NONE
                        });

                        newWordDatas.Add(new WordData
                        {
                            patternID = (int)MsbtEnums.WordDataPatternID.WordTag,
                            eventID = (int)MsbtEnums.MsgEventID.NONE,

                            // IMPORTANT:
                            // This points to tagDatas[tagDataIndex],
                            // NOT the tag's actual tagIndex.
                            tagIndex = tagDataIndex,

                            tagValue = 0,
                            str = "",
                            strWidth = -1f
                        });

                        s = s[(closeIndex + 1)..];

                        continue;
                    }

                    // ============================================================
                    // Escape sequence
                    // ============================================================

                    Match m = Regex.Match(
                        s,
                        @"\A[^\\]*\\"
                    );

                    if (m.Success)
                    {
                        int eventCharIndex = m.Value.Length;
                        if (eventCharIndex >= s.Length)
                        {
                            throw new ArgumentException(
                                "Incomplete macro at end of string."
                            );
                        }

                        char c = s[eventCharIndex];

                        if (c == '0' || 
                            c == 'n' ||
                            c == 'w' ||
                            c == 'r' ||
                            c == 'f' ||
                            c == 'e')
                        {
                            // Valid macro
                            WordData wd = new()
                            {
                                patternID = (int)MsbtEnums.WordDataPatternID.Event,
                                tagIndex = -1
                            };

                            wd.eventID = c switch
                            {
                                '0' => (int)MsbtEnums.MsgEventID.NONE,
                                'n' => (int)MsbtEnums.MsgEventID.NewLine,
                                'w' => (int)MsbtEnums.MsgEventID.Wait,
                                'r' => (int)MsbtEnums.MsgEventID.ScrollPage,
                                'f' => (int)MsbtEnums.MsgEventID.ScrollLine,
                                'e' => (int)MsbtEnums.MsgEventID.CallBack,

                                _ => throw new ArgumentException(
                                    "Unknown macro: \\" + c
                                ),
                            };

                            wd.str = s[..(m.Value.Length - 1)];

                            wd.strWidth = WordData.CalculateStringWidth(
                                wd.str,
                                out _);

                            newWordDatas.Add(wd);

                            s = s[(eventCharIndex + 1)..];

                            continue;
                        }
                    }

                    // ============================================================
                    // Plain text
                    // ============================================================

                    WordData textData = new()
                    {
                        patternID = (int)MsbtEnums.WordDataPatternID.Str,
                        eventID = (int)MsbtEnums.MsgEventID.End,
                        tagIndex = -1,
                        tagValue = 0,
                        str = s
                    };

                    textData.strWidth = WordData.CalculateStringWidth(
                        textData.str,
                        out _);

                    newWordDatas.Add(textData);

                    s = "";
                }

                wordDatas = newWordDatas;
            }

        }

        public class WordData : ICloneable
        {
            public int patternID;
            public int eventID;
            public int tagIndex;
            public float tagValue;
            public string str;
            public float strWidth;

            public static readonly Dictionary<char, float> charWidths = new()
            {
                { ' ', 8.671875f },
                { '\u00A0', 8.671875f }, // Non-breaking space

                { 'A', 20.125f },
                { 'À', 20.125f },
                { 'Á', 20.125f },
                { 'Â', 20.125f },
                { 'Ã', 20.125f },
                { 'Ä', 20.125f },
                { 'Å', 20.125f },
                { 'Æ', 20.125f },
                { 'B', 17.3125f },
                { 'C', 20.25f },
                { 'Ç', 20.25f },
                { 'D', 22.109375f },
                { 'E', 15.84375f },
                { 'È', 15.84375f },
                { 'É', 15.84375f },
                { 'Ê', 15.84375f },
                { 'Ë', 15.84375f },
                { 'F', 16.15625f },
                { 'G', 23.328125f },
                { 'H', 22.015625f },
                { 'I', 8.390625f },
                { 'Ì', 8.390625f },
                { 'Í', 8.390625f },
                { 'Î', 8.390625f },
                { 'Ï', 8.390625f },
                { 'J', 12.640625f },
                { 'K', 19.046875f },
                { 'L', 14.96875f },
                { 'M', 25.984375f },
                { 'N', 21.625f },
                { 'Ñ', 21.625f },
                { 'O', 24.390625f },
                { 'Ò', 24.390625f },
                { 'Ó', 24.390625f },
                { 'Ô', 24.390625f },
                { 'Õ', 24.390625f },
                { 'Ö', 24.390625f },
                { 'Ø', 24.390625f },
                { 'P', 16.28125f },
                { 'Q', 24.390625f },
                { 'R', 17.625f },
                { 'S', 15.453125f },
                { 'T', 17.125f },
                { 'U', 21.34375f },
                { 'Ù', 21.34375f },
                { 'Ú', 21.34375f },
                { 'Û', 21.34375f },
                { 'Ü', 21.34375f },
                { 'V', 20.0f },
                { 'W', 28.640625f },
                { 'X', 20.28125f },
                { 'Y', 19.328125f },
                { 'Ý', 19.328125f },
                { 'Z', 18.171875f },

                { 'a', 15.296875f },
                { 'à', 15.296875f },
                { 'á', 15.296875f },
                { 'â', 15.296875f },
                { 'ã', 15.296875f },
                { 'ä', 15.296875f },
                { 'å', 15.296875f },
                { 'æ', 15.296875f },
                { 'b', 17.25f },
                { 'c', 13.953125f },
                { 'ç', 13.953125f },
                { 'd', 17.28125f },
                { 'e', 15.96875f },
                { 'è', 15.96875f },
                { 'é', 15.96875f },
                { 'ê', 15.96875f },
                { 'ë', 15.96875f },
                { 'f', 9.765625f },
                { 'g', 16.1875f },
                { 'h', 15.578125f },
                { 'i', 7.609375f },
                { 'ì', 7.609375f },
                { 'í', 7.609375f },
                { 'î', 7.609375f },
                { 'ï', 7.609375f },
                { 'j', 7.328125f },
                { 'k', 14.8125f },
                { 'l', 7.78125f },
                { 'm', 22.71875f },
                { 'n', 15.578125f },
                { 'ñ', 15.578125f },
                { 'o', 17.15625f },
                { 'ò', 17.15625f },
                { 'ó', 17.15625f },
                { 'ô', 17.15625f },
                { 'õ', 17.15625f },
                { 'ö', 17.15625f },
                { 'ø', 17.15625f },
                { 'p', 17.25f },
                { 'q', 17.28125f },
                { 'r', 9.65625f },
                { 's', 11.515625f },
                { 't', 10.046875f },
                { 'u', 15.578125f },
                { 'ù', 15.578125f },
                { 'ú', 15.578125f },
                { 'û', 15.578125f },
                { 'ü', 15.578125f },
                { 'v', 14.078125f },
                { 'w', 19.8125f },
                { 'x', 14.46875f },
                { 'y', 14.375f },
                { 'ý', 14.375f },
                { 'ÿ', 14.375f },
                { 'z', 13.03125f },
                { 'ß', 16.734375f },

                { '1', 13.984375f },
                { '2', 17.984375f },
                { '3', 17.984375f },
                { '4', 17.984375f },
                { '5', 17.984375f },
                { '6', 17.984375f },
                { '7', 17.984375f },
                { '8', 17.984375f },
                { '9', 17.984375f },
                { '0', 17.984375f },

                { '－', 25.593750f },
                { '-', 11.203125f },
                { '–', 12.796875f },
                { '—', 25.593750f },
                { '―', 25.593750f },
                { 'ー', 25.593750f },
                { '_', 12.796875f },

                { '±', 25.593750f },
                { '+', 16.0625f },
                { '−', 26.593750f },
                { '=', 15.906250f },
                { '%', 23.078125f },
                { '*', 12.765625f },
                { '×', 25.593750f },
                { '/', 12.796875f },

                { '.', 7.906250f },
                { '·', 7.906250f },
                { '•', 10.750000f },
                { '●', 25.593750f },
                { '!', 7.265625f },
                { '¡', 7.265625f },
                { '?', 14.468750f },
                { '¿', 14.468750f },

                { '\'', 6.4609375f },
                { '"', 10.6640625f },
                { '“', 13.03125f },
                { '”', 13.03125f },
                { '„', 13.03125f },
                { '«', 15.9375f },
                { '»', 15.9375f },
                { '‘', 8.828125f },
                { '‚', 8.828125f },
                { ',', 8.828125f },
                { '’', 8.828125f },

                { '♀', 25.593750f },
                { '♂', 25.593750f },
                { '(', 11.359375f },
                { ')', 11.359375f },
                { ':', 10.109375f },
                { ';', 10.109375f },
                { '：', 25.593750f },
                { '&', 19.843750f },
                { 'ª', 8.953125f },
                { 'ᵉ', 8.828125f },
                { 'ō', 17.156250f },
                { 'Œ', 28.703125f },
                { 'œ', 27.359375f },

                { '↑', 25.593750f },
                { '→', 25.593750f },
                { '←', 25.593750f },
                { '↓', 25.593750f },
                { '★', 25.593750f },
                { '♥', 25.593750f },
                { '♪', 25.593750f },

                { 'ヒ', 25.593750f },
                { 'フ', 25.593750f },
                { 'ヘ', 25.593750f },
                { 'ホ', 25.593750f },
                { 'マ', 25.593750f },
                { 'ミ', 25.593750f },
                { 'ム', 25.593750f },
                { 'メ', 25.593750f },
                { 'モ', 25.593750f },
                { 'ヤ', 25.593750f },
                { 'ユ', 25.593750f },
                { 'ヨ', 25.593750f },
                { 'ラ', 25.593750f },
                { 'リ', 25.593750f },
                { 'ル', 25.593750f },
                { 'レ', 25.593750f },
                { 'ロ', 25.593750f },
                { 'ワ', 25.593750f },

                { '\uE104', 32.000000f },  // ✨
                { '\uE300', 21.125000f },  // Pokédollar symbol
                { '\u202F', 4.421875f },   // Narrow no-break space
                { '\u3000', 25.593750f },  // Ideographic space
            };

            public static float CalculateStringWidth(
                string inputString,
                out List<char> missingChars,
                bool debug = false,
                bool collectMissing = false)
            {
                float total = 0f;
                missingChars = new List<char>();

                foreach (char originalChar in inputString)
                {
                    // Match the Python behavior:
                    // if char == "'": char = "’"
                    char c = originalChar == '\'' ? '’' : originalChar;

                    if (charWidths.TryGetValue(c, out float width))
                    {
                        total += width;
                    }
                    else
                    {
                        if (debug)
                        {
                            Console.Error.WriteLine(
                                $"Warning: Character '{c}' not found in charWidths. " +
                                "Using width of space.");
                        }

                        if (collectMissing)
                            missingChars.Add(c);

                        total += charWidths[' '];
                    }
                }

                return total;
            }

            public string GetEndChar()
            {
                return ((MsbtEnums.MsgEventID)eventID) switch
                {
                    MsbtEnums.MsgEventID.NONE => "\\0", //No marker
                    MsbtEnums.MsgEventID.NewLine => "\n", //New line marker
                    MsbtEnums.MsgEventID.Wait => "", //Wait marker
                    MsbtEnums.MsgEventID.ScrollPage => "\n", //New textbox marker
                    MsbtEnums.MsgEventID.ScrollLine => "\n", //Scroll textbox marker
                    MsbtEnums.MsgEventID.CallBack => "", //Start/join event marker?
                    MsbtEnums.MsgEventID.GuidIcon => "", //Guid Icon marker
                    MsbtEnums.MsgEventID.End => "", //End of message
                    _ => "\0", //Unknown
                };
            }

            public string GetMacro()
            {
                return ((MsbtEnums.MsgEventID)eventID) switch
                {
                    MsbtEnums.MsgEventID.NONE => "\\0", //No marker
                    MsbtEnums.MsgEventID.NewLine => "\\n", //New line marker
                    MsbtEnums.MsgEventID.Wait => "\\w", //Wait marker
                    MsbtEnums.MsgEventID.ScrollPage => "\\r", //New textbox marker
                    MsbtEnums.MsgEventID.ScrollLine => "\\f", //Scroll textbox marker
                    MsbtEnums.MsgEventID.CallBack => "\\e", //Start/join event marker?

                    // GuidIcon doesn't have a macro reprensentation yet
                    MsbtEnums.MsgEventID.GuidIcon => "",

                    MsbtEnums.MsgEventID.End => "", //End of message
                    _ => throw new ArgumentException(
                        $"Unknown event ID: {eventID}"
                    ),
                };
            }

            public string GetMacro(TagData tagData)
            {
                if (IsWordTag())
                {
                    return "{" +
                        tagData.tagIndex +
                        "," +
                        tagData.groupID +
                        "," +
                        tagData.tagID +
                        "," +
                        tagData.tagParameter +
                        "}";
                }

                if (IsHtmlTag())
                    return str;

                return GetMacro();
            }

            public bool IsWordTag()
            {
                return patternID == (int)MsbtEnums.WordDataPatternID.WordTag;
            }

            public bool IsHtmlTag()
            {
                return patternID == (int)MsbtEnums.WordDataPatternID.FontTag ||
                    patternID == (int)MsbtEnums.WordDataPatternID.ColorTag ||
                    patternID == (int)MsbtEnums.WordDataPatternID.SizeTag ||
                    patternID == (int)MsbtEnums.WordDataPatternID.CtrlTag;
            }

            public bool IsString()
            {
                return patternID == (int)MsbtEnums.WordDataPatternID.Str;
            }

            public object Clone()
            {
                return MemberwiseClone();
            }
        }

        public class Pokemon : INamedEntity, ICloneable, IComparable<Pokemon>
        {
            public byte validFlag;
            public ushort personalID;
            public ushort dexID;
            public ushort formIndex;
            public byte formMax;
            public byte color;
            public ushort graNo;
            public byte basicHp;
            public byte basicAtk;
            public byte basicDef;
            public byte basicSpd;
            public byte basicSpAtk;
            public byte basicSpDef;
            public byte typingID1;
            public byte typingID2;
            public byte getRate;
            public byte rank;
            public ushort expValue;
            public ushort item1;
            public ushort item2;
            public ushort item3;
            public byte sex;
            public byte eggBirth;
            public byte initialFriendship;
            public byte eggGroup1;
            public byte eggGroup2;
            public byte grow;
            public ushort abilityID1;
            public ushort abilityID2;
            public ushort abilityID3;
            public ushort giveExp;
            public ushort height;
            public ushort weight;
            public ushort chihouZukanNo;
            public uint machine1;
            public uint machine2;
            public uint machine3;
            public uint machine4;
            public uint hiddenMachine;
            public ushort eggMonsno;
            public ushort eggFormno;
            public ushort eggFormnoKawarazunoishi;
            public byte eggFormInheritKawarazunoishi;

            public List<LevelUpMove> levelUpMoves;
            public List<ushort> eggMoves;
            public List<EvolutionPath> evolutionPaths;

            public TMLearnset externalTMLearnset;

            //Readonly
            public string name;
            public int formID;
            public (ushort wild, ushort trainer) pastEvoLvs;
            public (ushort wild, ushort trainer) nextEvoLvs;
            public List<Pokemon> pastPokemon;
            public List<Pokemon> nextPokemon;
            public List<Pokemon> inferiorForms;
            public List<Pokemon> superiorForms;
            public bool legendary;

            public object Clone()
            {
                Pokemon p = (Pokemon)MemberwiseClone();
                p.levelUpMoves = new();
                foreach (LevelUpMove lum in levelUpMoves)
                    p.levelUpMoves.Add((LevelUpMove)lum.Clone());
                p.eggMoves = new();
                p.eggMoves.AddRange(eggMoves);
                p.evolutionPaths = new();
                foreach (EvolutionPath ep in evolutionPaths)
                    p.evolutionPaths.Add((EvolutionPath)ep.Clone());

                return p;
            }

            public List<int> GetEggGroups()
            {
                List<int> l = new();
                l.Add(eggGroup1);
                if (eggGroup1 != eggGroup2)
                    l.Add(eggGroup2);
                return l;
            }

            public void SetEggGroups(List<int> l)
            {
                eggGroup1 = (byte)l[0];
                eggGroup2 = (byte)l[0];
                if (l.Count > 1)
                    eggGroup2 = (byte)l[1];
            }

            public int GetBST()
            {
                return basicHp + basicAtk + basicDef + basicSpAtk + basicSpDef + basicSpd;
            }

            public byte[] GetStats()
            {
                return new byte[]
                {
                    basicHp,
                    basicAtk,
                    basicDef,
                    basicSpAtk,
                    basicSpDef,
                    basicSpd
                };
            }

            public void SetStats(byte[] stats)
            {
                basicHp = stats[0];
                basicAtk = stats[1];
                basicDef = stats[2];
                basicSpAtk = stats[3];
                basicSpDef = stats[4];
                basicSpd = stats[5];
            }

            public List<int> GetTyping()
            {
                List<int> l = new();
                l.Add(typingID1);
                if (typingID1 != typingID2)
                    l.Add(typingID2);
                return l;
            }

            public void SetTyping(List<int> l)
            {
                typingID1 = (byte)l[0];
                typingID2 = (byte)l[0];
                if (l.Count > 1)
                    typingID2 = (byte)l[1];
            }

            public bool[] GetTMCompatibility()
            {
                if (externalTMLearnset != null)
                    return externalTMLearnset.GetTMCompatibility();
                bool[] tmCompatibility = new bool[128];
                for (int i = 0; i < 32; i++)
                    tmCompatibility[i] = (machine1 & ((uint)1 << i)) != 0;
                for (int i = 0; i < 32; i++)
                    tmCompatibility[i + 32] = (machine2 & ((uint)1 << i)) != 0;
                for (int i = 0; i < 32; i++)
                    tmCompatibility[i + 64] = (machine3 & ((uint)1 << i)) != 0;
                for (int i = 0; i < 32; i++)
                    tmCompatibility[i + 96] = (machine4 & ((uint)1 << i)) != 0;
                return tmCompatibility;
            }

            public void SetTMCompatibility(bool[] tmCompatibility)
            {
                externalTMLearnset?.SetTMCompatibility(tmCompatibility);
                machine1 = 0;
                for (int i = 0; i < 32; i++)
                    machine1 |= tmCompatibility[i] ? (uint)1 << i : 0;
                machine2 = 0;
                for (int i = 0; i < 32; i++)
                    machine2 |= tmCompatibility[i + 32] ? (uint)1 << i : 0;
                machine3 = 0;
                for (int i = 0; i < 32; i++)
                    machine3 |= tmCompatibility[i + 64] ? (uint)1 << i : 0;
                machine4 = 0;
                for (int i = 0; i < 32; i++)
                    machine4 |= tmCompatibility[i + 96] ? (uint)1 << i : 0;
            }

            public List<int> GetCompatibleTMs()
            {
                List<int> compatibleTMs = new();
                bool[] tmCompatibility = GetTMCompatibility();
                for (int tmID = 0; tmID < tmCompatibility.Length; tmID++)
                    if (tmCompatibility[tmID])
                        compatibleTMs.Add(tmID);
                return compatibleTMs;
            }

            public void SetCompatibleTMs(List<int> compatibleTMs)
            {
                bool[] tmCompatibility = new bool[128];
                for (int tmID = 0; tmID < tmCompatibility.Length; tmID++)
                    tmCompatibility[tmID] = compatibleTMs.Contains(tmID);
                SetTMCompatibility(tmCompatibility);
            }

            public int[] GetWildHeldItems()
            {
                return new int[3]
                {
                    item1,
                    item2,
                    item3
                };
            }

            public int[] GetAbilities()
            {
                return new int[3]
                {
                    abilityID1,
                    abilityID2,
                    abilityID3
                };
            }

            public int[] GetEvYield()
            {
                int[] evYield = new int[6];
                for (int i = 0; i < 6; i++)
                    evYield[i] = (expValue & (3 << (2 * i))) >> (2 * i);
                return evYield;
            }

            public void SetEvYield(int[] evYield)
            {
                expValue = 0;
                for (int i = 0; i < 6; i++)
                    expValue |= (ushort)(evYield[i] << (2 * i));
            }

            public int GetID()
            {
                return personalID;
            }

            public string GetName()
            {
                return name;
            }

            public bool IsValid()
            {
                return validFlag != 0 && personalID > 0;
            }

            public int CompareTo(Pokemon other)
            {
                if (formID.CompareTo(other.formID) != 0)
                {
                    if (formID == 0)
                        return -1;
                    if (other.formID == 0)
                        return 1;
                }

                int i = dexID.CompareTo(other.dexID);
                if (i == 0)
                    i = formID.CompareTo(other.formID);
                return i;
            }
        }

        public class DexEntry : INamedEntity
        {
            public int dexID;
            public List<Pokemon> forms;
            public string name;

            public List<DexEntry> GetPastEntries()
            {
                List<DexEntry> past = new();
                foreach (Pokemon pokemon in forms)
                    past = past.Union(pokemon.pastPokemon.Select(p => GlobalData.gameData.dexEntries[p.dexID])).ToList();

                return past;
            }

            public List<DexEntry> GetNextEntries()
            {
                List<DexEntry> next = new();
                foreach (Pokemon pokemon in forms)
                    next = next.Union(pokemon.nextPokemon.Select(p => GlobalData.gameData.dexEntries[p.dexID])).ToList();

                return next;
            }

            public int GetID()
            {
                return dexID;
            }

            public string GetName()
            {
                return name;
            }

            public bool IsValid()
            {
                return forms[0].IsValid();
            }
        }

        public class EvolutionPath : ICloneable
        {
            public ushort method;
            public ushort parameter;
            public ushort destDexID;
            public ushort destFormID;
            public ushort level;

            public object Clone()
            {
                return MemberwiseClone();
            }
        }

        public class LevelUpMove : ICloneable
        {
            public ushort level;
            public ushort moveID;

            public object Clone()
            {
                return MemberwiseClone();
            }
        }

        public class Item : GameDataTypes.INamedEntity
        {
            public short itemID;
            public byte type;
            public int iconID;
            public int price;
            public int bpPrice;
            public byte nageAtc;
            public byte sizenAtc;
            public byte sizenType;
            public byte tuibamuEff;
            public byte sort;
            public byte group;
            public byte groupID;
            public byte fldPocket;
            public byte fieldFunc;
            public byte battleFunc;
            public byte criticalRanks;
            public byte atkStages;
            public byte defStages;
            public byte spdStages;
            public byte accStages;
            public byte spAtkStages;
            public byte spDefStages;
            public byte ppRestoreAmount;
            public sbyte hpEvIncrease;
            public sbyte atkEvIncrease;
            public sbyte defEvIncrease;
            public sbyte spdEvIncrease;
            public sbyte spAtkEvIncrease;
            public sbyte spDefEvIncrease;
            public sbyte friendshipIncrease1; //0-99
            public sbyte friendshipIncrease2; //100-199
            public sbyte friendshipIncrease3; //200-255
            public byte hpRestoreAmount;
            public uint flags0;

            //Readonly
            public string name;

            public bool IsActive()
            {
                return !GetFlags()[31];
            }

            public bool[] GetFlags()
            {
                bool[] flags = new bool[32];
                for (int i = 0; i < 32; i++)
                    flags[i] = (flags0 & ((uint)1 << i)) != 0;
                return flags;
            }

            public void SetFlags(bool[] flagArray)
            {
                flags0 = 0;
                for (int i = 0; i < 32; i++)
                    flags0 |= flagArray[i] ? (uint)1 << i : 0;
            }

            public bool IsPurchasable()
            {
                return IsActive() && price > 0;
            }

            public int GetID()
            {
                return itemID;
            }

            public string GetName()
            {
                return name;
            }

            public bool IsValid()
            {
                return IsPurchasable();
                //Not completely accurate, but randomization is easier this way.
            }
        }

        public class TM : GameDataTypes.INamedEntity
        {
            public int itemID;
            public int machineNo;
            public int moveID;

            //Readonly
            public int tmID;
            public string name;

            public string GetFullName()
            {
                return GetName() + " " + GlobalData.gameData.moves[moveID].GetName();
            }

            public int GetID()
            {
                return tmID;
            }

            public string GetName()
            {
                return name;
            }

            public bool IsValid()
            {
                return GlobalData.gameData.items[itemID].IsActive() &&
                    GlobalData.gameData.items[itemID].fieldFunc == 2 &&
                    GlobalData.gameData.items[itemID].groupID <= GlobalData.gameData.GetTMCompatibilitySetSize() &&
                    GlobalData.gameData.items[itemID].groupID > 0;
            }
        }

        public class Move : GameDataTypes.INamedEntity
        {
            public int moveID;
            public byte isValid;
            public byte typingID;
            public byte category;
            public byte damageCategoryID;
            public byte power;
            public byte hitPer;
            public byte basePP;
            public sbyte priority;
            public byte hitCountMax;
            public byte hitCountMin;
            public ushort sickID;
            public byte sickPer;
            public byte sickCont;
            public byte sickTurnMin;
            public byte sickTurnMax;
            public byte criticalRank;
            public byte shrinkPer;
            public ushort aiSeqNo;
            public sbyte damageRecoverRatio;
            public sbyte hpRecoverRatio;
            public byte target;
            public byte rankEffType1;
            public byte rankEffType2;
            public byte rankEffType3;
            public sbyte rankEffValue1;
            public sbyte rankEffValue2;
            public sbyte rankEffValue3;
            public byte rankEffPer1;
            public byte rankEffPer2;
            public byte rankEffPer3;
            public uint flags;
            public uint contestWazaNo;

            public string cmdSeqName;
            public string cmdSeqNameLegend;
            public string notShortenTurnType0;
            public string notShortenTurnType1;
            public string turnType1;
            public string turnType2;
            public string turnType3;
            public string turnType4;

            //Readonly
            public string name;

            public bool[] GetFlags()
            {
                bool[] flagArray = new bool[32];
                for (int i = 0; i < 32; i++)
                    flagArray[i] = (flags & ((uint)1 << i)) != 0;
                return flagArray;
            }

            public void SetFlags(bool[] flagArray)
            {
                flags = 0;
                for (int i = 0; i < 32; i++)
                    flags |= flagArray[i] ? (uint)1 << i : 0;
            }

            public int GetID()
            {
                return moveID;
            }

            public string GetName()
            {
                return name;
            }

            public bool IsValid()
            {
                return isValid != 0 && moveID > 0;
            }
        }

        public class GrowthRate : INamedEntity
        {
            public int growthID;
            public List<uint> expRequirements;

            public int GetID()
            {
                return growthID;
            }

            public string GetName()
            {
                return growthID switch
                {
                    0 => "Medium Fast",
                    1 => "Erratic",
                    2 => "Fluctuating",
                    3 => "Medium Slow",
                    4 => "Fast",
                    5 => "Slow",
                    _ => ""
                };
            }

            public bool IsValid()
            {
                return growthID <= 5;
            }
        }

        public class UgArea
        {
            public int id;
            public string fileName;
        }

        public class UgEncounterFile
        {
            public string mName;
            public List<UgEncounter> ugEncounters;
        }

        public class UgEncounter
        {
            public int dexID;
            public int version;
            public int zukanFlag;
        }

        public class UgEncounterLevelSet
        {
            public int minLv;
            public int maxLv;

            public double GetAvgLevel()
            {
                return (minLv + maxLv) / 2.0;
            }
        }

        public class UgSpecialEncounter
        {
            public int id;
            public int dexID;
            public int version;
            public int dRate;
            public int pRate;
        }

        public class UgPokemonData : ICloneable
        {
            public int monsno;
            public int type1ID;
            public int type2ID;
            public int size;
            public int movetype;
            public int[] reactioncode;
            public int[] moveRate;
            public int[] submoveRate;
            public int[] reaction;
            public int[] flagrate;
            public int rateup;

            public object Clone()
            {
                UgPokemonData ugpd = (UgPokemonData)MemberwiseClone();
                ugpd.reactioncode = new int[2];
                for (int i = 0; i < ugpd.reactioncode.Length; i++)
                    ugpd.reactioncode[i] = reactioncode[i];
                ugpd.moveRate = new int[2];
                for (int i = 0; i < ugpd.moveRate.Length; i++)
                    ugpd.moveRate[i] = moveRate[i];
                ugpd.submoveRate = new int[5];
                for (int i = 0; i < ugpd.submoveRate.Length; i++)
                    ugpd.submoveRate[i] = submoveRate[i];
                ugpd.reaction = new int[5];
                for (int i = 0; i < ugpd.reaction.Length; i++)
                    ugpd.reaction[i] = reaction[i];
                ugpd.flagrate = new int[6];
                for (int i = 0; i < ugpd.flagrate.Length; i++)
                    ugpd.flagrate[i] = flagrate[i];
                return ugpd;
            }
        }

        public class Ability : INamedEntity
        {
            public int abilityID;
            public string name;

            public int GetID()
            {
                return abilityID;
            }

            public string GetName()
            {
                return name;
            }

            public bool IsValid()
            {
                return abilityID > 0;
            }
        }

        public class Typing : INamedEntity
        {
            public int typingID;
            public string name;

            public int GetID()
            {
                return typingID;
            }

            public string GetName()
            {
                return name;
            }

            public bool IsValid()
            {
                return true;
            }
        }

        public class DamageCategory : INamedEntity
        {
            public int damageCategoryID;

            public int GetID()
            {
                return damageCategoryID;
            }

            public string GetName()
            {
                return damageCategoryID switch
                {
                    0 => "Status",
                    1 => "Physical",
                    2 => "Special",
                    _ => null,
                };
            }

            public bool IsValid()
            {
                return damageCategoryID > 0;
            }
        }

        public class Nature : INamedEntity
        {
            public int natureID;
            public string name;

            public int GetID()
            {
                return natureID;
            }

            public string GetName()
            {
                return name;
            }

            public bool IsValid()
            {
                return true;
            }
        }

        public class ResultMotion : ICloneable, IComparable<ResultMotion>
        {
            public byte validFlag;
            public ushort id;
            public int monsNo;
            public uint winAnim;
            public uint loseAnim;
            public uint waitAnim;
            public float duration;

            public object Clone()
            {
                return MemberwiseClone();
            }

            public int CompareTo(ResultMotion other)
            {
                return id - other.id;
            }
        }

        public class GlobalMetadata
        {
            public byte[] buffer;

            //Readonly
            public uint stringOffset;
            public uint defaultValuePtrOffset;
            public uint defaultValuePtrSecSize;
            public uint defaultValueOffset;
            public uint defaultValueSecSize;
            public uint fieldOffset;
            public uint typeOffset;
            public uint imageOffset;
            public uint imageSecSize;

            public Dictionary<uint, FieldDefaultValue> defaultValueDic;
            public List<ImageDefinition> images;

            public long[] typeMatchupOffsets;

            public byte GetTypeMatchup(int off, int def)
            {
                return buffer[typeMatchupOffsets[off] + def];
            }

            public void SetTypeMatchup(int off, int def, byte aff)
            {
                buffer[typeMatchupOffsets[off] + def] = aff;
            }
        }

        public class ImageDefinition : IGMObject
        {
            public string name;
            public uint typeStart;
            public uint typeCount;
            public List<TypeDefinition> types;

            public override string ToString()
            {
                return name;
            }

            public bool HasDefault()
            {
                return types.Any(t => t.HasDefault());
            }
        }

        public class TypeDefinition : IGMObject
        {
            public string name;
            public int fieldStart;
            public ushort fieldCount;
            public List<FieldDefinition> fields;

            public override string ToString()
            {
                return name;
            }

            public bool HasDefault()
            {
                return fields.Any(f => f.HasDefault());
            }
        }

        public class FieldDefinition : IGMObject
        {
            public string name;
            public FieldDefaultValue defautValue;

            public override string ToString()
            {
                return name;
            }

            public bool HasDefault()
            {
                return defautValue != null;
            }
        }

        public class FieldDefaultValue
        {
            public long offset;
            public int length;
        }

        public interface IGMObject
        {
            public bool HasDefault();
            public string ToString();
        }

        public interface INamedEntity
        {
            public int GetID();
            public string GetName();
            public bool IsValid();
        }

        public enum Language
        {
            Japanese = 1,
            English = 2,
            French = 3,
            Italian = 4,
            German = 5,
            Spanish = 7,
            Korean = 8,
            SimpChinese = 9,
            TradChinese = 10
        }
    }
}
