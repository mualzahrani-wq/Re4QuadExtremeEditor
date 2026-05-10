using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Re4QuadExtremeEditor.src.JSON;
using Re4QuadExtremeEditor.src.Class;
using Re4QuadExtremeEditor.src.Class.Enums;
using Re4QuadExtremeEditor.src.Class.TreeNodeObj;
using System.IO;
using OpenTK;

namespace Re4QuadExtremeEditor.src
{
    /// <summary>
    /// Utility methods used throughout the program.
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Returns default configuration values.
        /// </summary>
        public static Configs GetDefaultConfigs()
        {
            Configs configs = new Configs();
            configs.xfileDiretory = @"xfile\";
            configs.xscrDiretory = @"xscr\";
            configs.SkyColor = Color.Azure;
            configs.FrationalAmount = 9;
            configs.FrationalSymbol = ConfigFrationalSymbol.AcceptsCommaAndPeriod_OutputPeriod;
            configs.ItemDisableRotationAll = false;
            configs.ItemDisableRotationIfXorYorZequalZero = false;
            configs.ItemDisableRotationIfZisNotGreaterThanZero = true;
            configs.ItemRotationOrder = ObjRotationOrder.RotationXY;
            configs.ItemRotationCalculationMultiplier = 1;
            configs.ItemRotationCalculationDivider = 1;
            configs.ForceUseOldOpenGL = false;
            configs.ForceUseModernOpenGL = false;
            return configs;
        }

        /// <summary>
        /// Loads and applies saved configurations on startup.
        /// </summary>
        public static void StartLoadConfigs()
        {
            if (File.Exists(Consts.ConfigsFileDiretory))
            {
                Configs configs = GetDefaultConfigs();
                try { configs = ConfigsFile.parseConfigs(Consts.ConfigsFileDiretory); } catch (Exception) { }

                Globals.BackupConfigs = configs;
                Globals.xfileDiretory = configs.xfileDiretory;
                Globals.xscrDiretory = configs.xscrDiretory;
                Globals.SkyColor = configs.SkyColor;
                Globals.FrationalAmount = configs.FrationalAmount;
                Globals.FrationalSymbol = configs.FrationalSymbol;
                Globals.ItemDisableRotationAll = configs.ItemDisableRotationAll;
                Globals.ItemDisableRotationIfXorYorZequalZero = configs.ItemDisableRotationIfXorYorZequalZero;
                Globals.ItemDisableRotationIfZisNotGreaterThanZero = configs.ItemDisableRotationIfZisNotGreaterThanZero;
                Globals.ItemRotationOrder = configs.ItemRotationOrder;
                Globals.ItemRotationCalculationMultiplier = configs.ItemRotationCalculationMultiplier;
                Globals.ItemRotationCalculationDivider = configs.ItemRotationCalculationDivider;
            }
            else
            {
                if (!Directory.Exists(Consts.dataDiretory))
                    Directory.CreateDirectory(Consts.dataDiretory);

                try { ConfigsFile.writeConfigsFile(Consts.ConfigsFileDiretory, GetDefaultConfigs()); } catch (Exception) { }
                Globals.BackupConfigs = GetDefaultConfigs();
            }
        }

        /// <summary>
        /// Loads the room info list on startup.
        /// </summary>
        public static void StartLoadRoomInfoList()
        {
            try
            {
                DataBase.RoomList.Clear();
                DataBase.RoomList = RoomInfoFile.parseRoomList(Consts.RoomListFileDiretory);
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Loads GLSL shaders on startup.
        /// </summary>
        public static void StartLoadShader()
        {
            DataBase.ShaderRoom = new Class.Shaders.Shader(
                Encoding.UTF8.GetString(Properties.Resources.RoomShaderVert),
                Encoding.UTF8.GetString(Properties.Resources.RoomShaderFrag));
            DataBase.ShaderRoom.Use();
            DataBase.ShaderRoom.SetInt("texture0", 0);

            DataBase.ShaderObjs = new Class.Shaders.Shader(
                Encoding.UTF8.GetString(Properties.Resources.ObjShaderVert),
                Encoding.UTF8.GetString(Properties.Resources.ObjShaderFrag));
            DataBase.ShaderObjs.Use();
            DataBase.ShaderObjs.SetInt("texture0", 0);

            DataBase.ShaderBoundingBox = new Class.Shaders.Shader(
                Encoding.UTF8.GetString(Properties.Resources.BoundingBoxShaderVert),
                Encoding.UTF8.GetString(Properties.Resources.BoundingBoxShaderFrag));
            DataBase.ShaderBoundingBox.Use();
        }

        /// <summary>
        /// Initialises legacy (OpenGL 2/3) no-shader rendering path.
        /// </summary>
        public static void StartLoadNoShader_OldGL()
        {
            DataBase.ShaderRoom = new Class.Shaders.NoShaderRoom();
            DataBase.ShaderObjs = new Class.Shaders.NoShaderObjs();
            DataBase.ShaderBoundingBox = new Class.Shaders.NoShaderBoundingBox();
        }

        /// <summary>
        /// Loads 3-D object models on startup.
        /// </summary>
        public static void StartLoadObjsModels()
        {
            DataBase.InternalModels = new ModelGroup(Consts.InternalModelGroupName, Consts.InternalModelsJsonFilesDiretory, Consts.InternalModelsListFileDiretory, Directory.GetCurrentDirectory() + "\\");
            DataBase.ItemsModels    = new ModelGroup(Consts.ItemsModelGroupName,    Consts.ItemsModelsJsonFilesDiretory,    Consts.ItemsModelsListFileDiretory,    Globals.xfileDiretory);
            DataBase.EtcModels      = new ModelGroup(Consts.EtcModelGroupName,      Consts.EtcModelsJsonFilesDiretory,      Consts.EtcModelsListFileDiretory,      Globals.xfileDiretory);
            DataBase.EnemiesModels  = new ModelGroup(Consts.EnemiesModelGroupName,  Consts.EnemiesModelsJsonFilesDiretory,  Consts.EnemiesModelsListFileDiretory,  Globals.xfileDiretory);
        }

        /// <summary>
        /// Loads ObjInfo ID dictionaries on startup.
        /// </summary>
        public static void StartLoadObjsInfoLists()
        {
            DataBase.ItemsIDs    = new Dictionary<ushort, ObjInfo>();
            DataBase.EtcModelIDs = new Dictionary<ushort, ObjInfo>();
            DataBase.EnemiesIDs  = new Dictionary<ushort, ObjInfo>();
            try { DataBase.ItemsIDs    = ObjInfoFile.parseObjInfoList(Consts.ItemsObjInfoListFileDiretory);    } catch (Exception) { }
            try { DataBase.EtcModelIDs = ObjInfoFile.parseObjInfoList(Consts.EtcModelObjInfoListFileDiretory); } catch (Exception) { }
            try { DataBase.EnemiesIDs  = ObjInfoFile.parseObjInfoList(Consts.EnemiesObjInfoListFileDiretory);  } catch (Exception) { }
        }

        /// <summary>
        /// Expands the enemy ID list with extra silent segments (0x5000 and 0xA000 ranges).
        /// </summary>
        public static void StartEnemyExtraSegmentList()
        {
            if (!Globals.CreateEnemyExtraSegmentList) return;

            AddEnemySegment(0x5000, eLang.EnemyExtraSegmentSegund);
            AddEnemySegment(0xA000, eLang.EnemyExtraSegmentThird);
        }

        // Shared helper used by StartEnemyExtraSegmentList.
        private static void AddEnemySegment(ushort baseOffset, eLang segmentLang)
        {
            for (ushort i = 0; i < 0x50; i++)
            {
                ushort originalId = (ushort)(i * 0x100);
                var keys = (from obj in DataBase.EnemiesIDs
                            where obj.Key >= originalId && obj.Key <= (originalId + 0xFF)
                            select obj.Key).ToArray();

                foreach (var key in keys)
                {
                    ushort newId = (ushort)(key + baseOffset);
                    ObjInfo obj;
                    if (!DataBase.EnemiesIDs.ContainsKey(newId) && DataBase.EnemiesIDs.TryGetValue(key, out obj))
                    {
                        string suffix = Lang.GetText(segmentLang);
                        DataBase.EnemiesIDs.Add(newId, new ObjInfo(
                            newId,
                            obj.ModelKey,
                            obj.UseInternalModel,
                            obj.Name + " " + suffix,
                            obj.Description + " " + suffix + " " + Lang.GetText(eLang.EnemyExtraSegmentNoSound)));
                    }
                }
            }
        }

        /// <summary>
        /// Loads the prompt-message list used in list-box properties.
        /// </summary>
        public static void StartLoadPromptMessageList()
        {
            try
            {
                ListBoxProperty.PromptMessageList = PromptMessageListFile.parsePromptMessageList(Consts.PromptMessageListFileDiretory);
            }
            catch (Exception)
            {
                ListBoxProperty.PromptMessageList = new Dictionary<byte, ByteObjForListBox>();
            }
        }

        /// <summary>
        /// Populates static list-box dictionaries used by property grids.
        /// </summary>
        public static void StartSetListBoxsProperty()
        {
            // FloatType
            ListBoxProperty.FloatTypeList = new Dictionary<bool, BoolObjForListBox>
            {
                { false, new BoolObjForListBox(false, Lang.GetAttributeText(aLang.ListBoxFloatTypeDisable)) },
                { true,  new BoolObjForListBox(true,  Lang.GetAttributeText(aLang.ListBoxFloatTypeEnable)) },
            };

            // EnemyEnable
            ListBoxProperty.EnemyEnableList = new Dictionary<byte, ByteObjForListBox>
            {
                { 0x00, new ByteObjForListBox(0x00, "00: " + Lang.GetAttributeText(aLang.ListBoxDisable)) },
                { 0x01, new ByteObjForListBox(0x01, "01: " + Lang.GetAttributeText(aLang.ListBoxEnable)) },
            };

            // SpecialZoneCategory
            ListBoxProperty.SpecialZoneCategoryList = new Dictionary<byte, ByteObjForListBox>
            {
                { 0x00, new ByteObjForListBox(0x00, "00: " + Lang.GetAttributeText(aLang.ListBoxSpecialZoneCategory00)) },
                { 0x01, new ByteObjForListBox(0x01, "01: " + Lang.GetAttributeText(aLang.ListBoxSpecialZoneCategory01)) },
                { 0x02, new ByteObjForListBox(0x02, "02: " + Lang.GetAttributeText(aLang.ListBoxSpecialZoneCategory02)) },
            };

            // RefInteractionType
            ListBoxProperty.RefInteractionTypeList = new Dictionary<byte, ByteObjForListBox>
            {
                { 0x00, new ByteObjForListBox(0x00, "00: " + Lang.GetAttributeText(aLang.ListBoxRefInteractionType00)) },
                { 0x01, new ByteObjForListBox(0x01, "01: " + Lang.GetAttributeText(aLang.ListBoxRefInteractionType01Enemy)) },
                { 0x02, new ByteObjForListBox(0x02, "02: " + Lang.GetAttributeText(aLang.ListBoxRefInteractionType02EtcModel)) },
            };

            // ItemAuraType
            var itemAura = new Dictionary<ushort, UshortObjForListBox>();
            for (ushort a = 0x00; a <= 0x09; a++)
            {
                aLang key = (aLang)Enum.Parse(typeof(aLang), "ListBoxItemAuraType" + a.ToString("X2"));
                itemAura.Add(a, new UshortObjForListBox(a, a.ToString("X2") + ": " + Lang.GetAttributeText(key)));
            }
            ListBoxProperty.ItemAuraTypeList = itemAura;

            // SpecialType
            var specialTypes = new Dictionary<SpecialType, ByteObjForListBox>
            {
                { SpecialType.T00_GeneralPurpose,       new ByteObjForListBox(0x00, Lang.GetAttributeText(aLang.SpecialType00_GeneralPurpose)) },
                { SpecialType.T01_WarpDoor,             new ByteObjForListBox(0x01, Lang.GetAttributeText(aLang.SpecialType01_WarpDoor)) },
                { SpecialType.T02_CutSceneEvents,       new ByteObjForListBox(0x02, Lang.GetAttributeText(aLang.SpecialType02_CutSceneEvents)) },
                { SpecialType.T03_Items,                new ByteObjForListBox(0x03, Lang.GetAttributeText(aLang.SpecialType03_Items)) },
                { SpecialType.T04_GroupedEnemyTrigger,  new ByteObjForListBox(0x04, Lang.GetAttributeText(aLang.SpecialType04_GroupedEnemyTrigger)) },
                { SpecialType.T05_Message,              new ByteObjForListBox(0x05, Lang.GetAttributeText(aLang.SpecialType05_Message)) },
                { SpecialType.T08_TypeWriter,           new ByteObjForListBox(0x08, Lang.GetAttributeText(aLang.SpecialType08_TypeWriter)) },
                { SpecialType.T0A_DamagesThePlayer,     new ByteObjForListBox(0x0A, Lang.GetAttributeText(aLang.SpecialType0A_DamagesThePlayer)) },
                { SpecialType.T0B_FalseCollision,       new ByteObjForListBox(0x0B, Lang.GetAttributeText(aLang.SpecialType0B_FalseCollision)) },
                { SpecialType.T0D_Unknown,              new ByteObjForListBox(0x0D, Lang.GetAttributeText(aLang.SpecialType0D_Unknown)) },
                { SpecialType.T0E_Crouch,               new ByteObjForListBox(0x0E, Lang.GetAttributeText(aLang.SpecialType0E_Crouch)) },
                { SpecialType.T10_FixedLadderClimbUp,   new ByteObjForListBox(0x10, Lang.GetAttributeText(aLang.SpecialType10_FixedLadderClimbUp)) },
                { SpecialType.T11_ItemDependentEvents,  new ByteObjForListBox(0x11, Lang.GetAttributeText(aLang.SpecialType11_ItemDependentEvents)) },
                { SpecialType.T12_AshleyHideCommand,    new ByteObjForListBox(0x12, Lang.GetAttributeText(aLang.SpecialType12_AshleyHideCommand)) },
                { SpecialType.T13_LocalTeleportation,   new ByteObjForListBox(0x13, Lang.GetAttributeText(aLang.SpecialType13_LocalTeleportation)) },
                { SpecialType.T14_UsedForElevators,     new ByteObjForListBox(0x14, Lang.GetAttributeText(aLang.SpecialType14_UsedForElevators)) },
                { SpecialType.T15_AdaGrappleGun,        new ByteObjForListBox(0x15, Lang.GetAttributeText(aLang.SpecialType15_AdaGrappleGun)) },
            };
            ListBoxProperty.SpecialTypeList = specialTypes;
        }

        /// <summary>
        /// Populates the Enemies, EtcModels and Items list-box dictionaries.
        /// </summary>
        public static void StartSetListBoxsPropertybjsInfoLists()
        {
            ListBoxProperty.EnemiesList = BuildUshortListBox(
                DataBase.EnemiesIDs,
                id => { string h = id.ToString("X4"); return h[2] == 'F' && h[3] == 'F'; });

            ListBoxProperty.EtcmodelsList = BuildUshortListBox(DataBase.EtcModelIDs);
            ListBoxProperty.ItemsList      = BuildUshortListBox(DataBase.ItemsIDs);
        }

        // Shared helper: build an ordered UshortObjForListBox dictionary from an ObjInfo dictionary.
        private static Dictionary<ushort, UshortObjForListBox> BuildUshortListBox(
            Dictionary<ushort, ObjInfo> source,
            Func<ushort, bool> exclude = null)
        {
            var dict = new Dictionary<ushort, UshortObjForListBox>();
            foreach (var item in source)
            {
                if (item.Value.GameId == ushort.MaxValue) continue;
                if (exclude != null && exclude(item.Value.GameId)) continue;
                dict[item.Value.GameId] = new UshortObjForListBox(
                    item.Value.GameId,
                    item.Value.GameId.ToString("X4") + ": " + item.Value.Description);
            }
            return dict.OrderBy(o => o.Key).ToDictionary(p => p.Key, p => p.Value);
        }

        /// <summary>
        /// Creates the top-level TreeNode group nodes.
        /// </summary>
        public static void StartCreateNodes()
        {
            DataBase.NodeESL    = CreateEnemyGroup();
            DataBase.NodeETS    = CreateEtcModelGroup();
            DataBase.NodeITA    = CreateSpecialGroup(GroupType.ITA,    eLang.NodeITA,    Consts.NodeITA,    Globals.NodeColorITA);
            DataBase.NodeAEV    = CreateSpecialGroup(GroupType.AEV,    eLang.NodeAEV,    Consts.NodeAEV,    Globals.NodeColorAEV);
            DataBase.NodeEXTRAS = CreateExtraGroup();
        }

        private static EnemyNodeGroup CreateEnemyGroup()
        {
            var n = new EnemyNodeGroup();
            n.Group    = GroupType.ESL;
            n.Text     = Lang.GetText(eLang.NodeESL);
            n.Name     = Consts.NodeESL;
            n.ForeColor = Globals.NodeColorESL;
            n.NodeFont  = Globals.TreeNodeFontText;
            return n;
        }

        private static EtcModelNodeGroup CreateEtcModelGroup()
        {
            var n = new EtcModelNodeGroup();
            n.Group    = GroupType.ETS;
            n.Text     = Lang.GetText(eLang.NodeETS);
            n.Name     = Consts.NodeETS;
            n.ForeColor = Globals.NodeColorETS;
            n.NodeFont  = Globals.TreeNodeFontText;
            return n;
        }

        private static SpecialNodeGroup CreateSpecialGroup(GroupType group, eLang textKey, string name, System.Drawing.Color color)
        {
            var n = new SpecialNodeGroup();
            n.Group    = group;
            n.Text     = Lang.GetText(textKey);
            n.Name     = name;
            n.ForeColor = color;
            n.NodeFont  = Globals.TreeNodeFontText;
            return n;
        }

        private static ExtraNodeGroup CreateExtraGroup()
        {
            var n = new ExtraNodeGroup();
            n.Group    = GroupType.EXTRAS;
            n.Text     = Lang.GetText(eLang.NodeEXTRAS);
            n.Name     = Consts.NodeEXTRAS;
            n.ForeColor = Globals.NodeColorEXTRAS;
            n.NodeFont  = Globals.TreeNodeFontText;
            return n;
        }

        /// <summary>
        /// Initialises the ExtraGroup and wires it to the EXTRAS node.
        /// </summary>
        public static void StartExtraGroup()
        {
            DataBase.Extras = new Class.Files.ExtraGroup();
            DataBase.NodeEXTRAS.DisplayMethods = DataBase.Extras.DisplayMethods;
            DataBase.NodeEXTRAS.MoveMethods    = DataBase.Extras.MoveMethods;
        }

        public static List<ushort> AllUshots()
        {
            var list = new List<ushort>(ushort.MaxValue);
            for (ushort i = 0; i < ushort.MaxValue; i++)
                list.Add(i);
            return list;
        }

        public static OpenTK.Vector4 ColorToVector4(Color color)
        {
            return new OpenTK.Vector4(
                color.R / 255f,
                color.G / 255f,
                color.B / 255f,
                color.A / 255f);
        }

        public static Color Vector4ToColor(OpenTK.Vector4 color)
        {
            return Color.FromArgb(
                (int)Math.Round(color.W * 255f),
                (int)Math.Round(color.X * 255f),
                (int)Math.Round(color.Y * 255f),
                (int)Math.Round(color.Z * 255f));
        }

        public static float EnemyAngleToRad(short enemyAngle)
        {
            return (MathHelper.TwoPi * enemyAngle) / 32768f;
        }

        public static short RadToEnemyAngle(float radAngle)
        {
            float temp = (32768f * radAngle) / MathHelper.TwoPi;
            temp = Math.Max(short.MinValue, Math.Min(short.MaxValue, temp));
            return (short)temp;
        }

        /// <summary>
        /// Wraps a radian angle into the range [0, 2π) without using a loop.
        /// Fixes the original while-loop that could infinite-loop on extreme values.
        /// </summary>
        public static float RadAngle1Scale(float radAngle)
        {
            if (float.IsNaN(radAngle) || float.IsInfinity(radAngle)) return 0f;
            float twoPi = MathHelper.TwoPi;
            float result = radAngle % twoPi;
            if (result < 0f) result += twoPi;
            return result;
        }

        public static SpecialFileFormat GroupTypeToSpecialFileFormat(GroupType group)
        {
            switch (group)
            {
                case GroupType.ITA: return SpecialFileFormat.ITA;
                case GroupType.AEV: return SpecialFileFormat.AEV;
            }
            return SpecialFileFormat.NULL;
        }

        public static SpecialType ToSpecialType(byte specialType)
        {
            return specialType < 0x16 ? (SpecialType)specialType : SpecialType.UnspecifiedType;
        }

        public static void ToMoveCheckLimits(ref Vector3[] value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                Vector3 v = value[i];
                v.X = ClampFloat(v.X);
                v.Y = ClampFloat(v.Y);
                v.Z = ClampFloat(v.Z);
                value[i] = v;
            }
        }

        // Clamp a float to the safe range used for move coordinates.
        private static float ClampFloat(float f)
        {
            if (float.IsNaN(f) || float.IsInfinity(f)) return 0f;
            return Math.Max(-Consts.MyFloatMax, Math.Min(Consts.MyFloatMax, f));
        }

        public static Vector3[] GetObjScale_ToMove_Null(ushort ID) => null;
        public static void SetObjScale_ToMove_Null(ushort ID, Vector3[] value) { }

        /// <summary>
        /// Reloads all JSON data files and refreshes the property lists.
        /// </summary>
        public static void ReloadJsonFiles()
        {
            StartLoadRoomInfoList();
            StartLoadObjsInfoLists();
            StartSetTextTranslationLists();
            StartEnemyExtraSegmentList();
            StartSetListBoxsPropertybjsInfoLists();
        }

        /// <summary>
        /// Reloads all 3-D models and frees previous GPU resources.
        /// </summary>
        public static void ReloadModels()
        {
            DataBase.InternalModels.ClearGL();
            DataBase.ItemsModels.ClearGL();
            DataBase.EtcModels.ClearGL();
            DataBase.EnemiesModels.ClearGL();
            StartLoadObjsModels();
            if (DataBase.SelectedRoom != null)
            {
                DataBase.SelectedRoom.ClearGL();
                DataBase.SelectedRoom = null;
            }
            GC.Collect();
        }

        public static UshortObjForListBox[] ItemRotationOrderForListBox()
        {
            return new UshortObjForListBox[]
            {
                new UshortObjForListBox(0,  Lang.GetText(eLang.RotationXYZ)),
                new UshortObjForListBox(1,  Lang.GetText(eLang.RotationXZY)),
                new UshortObjForListBox(2,  Lang.GetText(eLang.RotationYXZ)),
                new UshortObjForListBox(3,  Lang.GetText(eLang.RotationYZX)),
                new UshortObjForListBox(4,  Lang.GetText(eLang.RotationZYX)),
                new UshortObjForListBox(5,  Lang.GetText(eLang.RotationZXY)),
                new UshortObjForListBox(6,  Lang.GetText(eLang.RotationXY)),
                new UshortObjForListBox(7,  Lang.GetText(eLang.RotationXZ)),
                new UshortObjForListBox(8,  Lang.GetText(eLang.RotationYX)),
                new UshortObjForListBox(9,  Lang.GetText(eLang.RotationYZ)),
                new UshortObjForListBox(10, Lang.GetText(eLang.RotationZX)),
                new UshortObjForListBox(11, Lang.GetText(eLang.RotationZY)),
                new UshortObjForListBox(12, Lang.GetText(eLang.RotationX)),
                new UshortObjForListBox(13, Lang.GetText(eLang.RotationY)),
                new UshortObjForListBox(14, Lang.GetText(eLang.RotationZ)),
            };
        }

        /// <summary>
        /// Loads available language list from disk.
        /// </summary>
        public static void StartLoadLangList()
        {
            if (Directory.Exists(Consts.langDiretory) && File.Exists(Consts.LangListFileDiretory))
                Globals.Langs.AddRange(JSON.LangListFile.parseLangList(Consts.LangListFileDiretory));
        }

        /// <summary>
        /// Loads the selected translation file.
        /// </summary>
        public static void StartLoadLangFile()
        {
            if (Globals.BackupConfigs.LoadLangTranslation)
            {
                LangObjForList lang = Globals.Langs.Find(l => l.LangID == Globals.BackupConfigs.LangID);
                if (lang != null && File.Exists(Consts.langDiretory + lang.LangFilePath))
                {
                    Lang.LoadedTranslation = true;
                    Lang.StartOthersTexts();
                    LangFile.parseLang(Consts.langDiretory + lang.LangFilePath);
                }
            }
        }

        /// <summary>
        /// Applies the loaded translation strings to all data lists.
        /// </summary>
        public static void StartSetTextTranslationLists()
        {
            if (!Lang.LoadedTranslation) return;

            ApplyTranslationToObjInfo(DataBase.EnemiesIDs,  Lang.Lists.Enemy);
            ApplyTranslationToObjInfo(DataBase.EtcModelIDs, Lang.Lists.EtcModel);
            ApplyTranslationToObjInfo(DataBase.ItemsIDs,    Lang.Lists.Item);

            foreach (var item in DataBase.RoomList)
            {
                if (Lang.Lists.Room.ContainsKey(item.RoomKey))
                {
                    var entry = Lang.Lists.Room[item.RoomKey];
                    if (entry.Key   != null) item.Name        = entry.Key;
                    if (entry.Value != null) item.Description = entry.Value;
                }
            }
        }

        // Shared helper: apply translation strings to an ObjInfo dictionary.
        private static void ApplyTranslationToObjInfo(
            Dictionary<ushort, ObjInfo> dict,
            Dictionary<string, System.Collections.Generic.KeyValuePair<string,string>> translations)
        {
            foreach (var item in dict)
            {
                string key = item.Key.ToString("X4");
                if (translations.ContainsKey(key))
                {
                    var entry = translations[key];
                    if (entry.Key   != null) item.Value.Name        = entry.Key;
                    if (entry.Value != null) item.Value.Description = entry.Value;
                }
            }
        }

        /// <summary>
        /// Detects the OpenGL version and sets Globals.UseOldGL accordingly.
        /// Forces modern or legacy path when the config overrides are set.
        /// </summary>
        public static void Defines_The_OpenGL_Used()
        {
            try
            {
                string glString = (Globals.OpenGLVersion ?? string.Empty).Trim();

                if (glString.StartsWith("1.") || glString.StartsWith("2.") || glString.StartsWith("3."))
                    Globals.UseOldGL = true;

                if (glString.StartsWith("1."))
                {
                    System.Windows.Forms.MessageBox.Show(
                        "Error: You have an outdated version of OpenGL, which is not supported by this program." +
                        " The program will now exit.\n\nOpenGL version: [" + Globals.OpenGLVersion + "]\n",
                        "OpenGL version error:",
                        System.Windows.Forms.MessageBoxButtons.OK,
                        System.Windows.Forms.MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(
                    "Error: " + ex.Message,
                    "Error detecting OpenGL version:",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);
            }

            // Config overrides take priority over auto-detection.
            if (Globals.BackupConfigs.ForceUseModernOpenGL) Globals.UseOldGL = false;
            if (Globals.BackupConfigs.ForceUseOldOpenGL)    Globals.UseOldGL = true;
        }
    }
}
