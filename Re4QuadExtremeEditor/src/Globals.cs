using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using OpenTK;
using Re4QuadExtremeEditor.src.Class.Enums;

namespace Re4QuadExtremeEditor.src
{
    /// <summary>
    /// Representa todos os status (configurações/opções) do programa;
    /// </summary>
    public static class Globals
    {

        #region Configs

        // diretorio dos arquivos pmds das rooms.
        public static string xscrDiretory = @"xscr/";

        // diretorio de todos os objetos, itens, etcmodel, inimigos.
        public static string xfileDiretory = @"xfile/";

        // a cor do ceu
        public static Color SkyColor = Color.Azure;

        // float
        public static ConfigFrationalSymbol FrationalSymbol = ConfigFrationalSymbol.AcceptsCommaAndPeriod_OutputPeriod;
        public static int FrationalAmount = 9;

        // itens rotations options
        public static bool ItemDisableRotationAll = false;
        public static bool ItemDisableRotationIfXorYorZequalZero = false;
        public static bool ItemDisableRotationIfZisNotGreaterThanZero = true;
        public static ObjRotationOrder ItemRotationOrder = ObjRotationOrder.RotationXY;
        public static float ItemRotationCalculationMultiplier = 1;
        public static float ItemRotationCalculationDivider = 1;

        #endregion

        #region Colors

        // cores
        public static Color NodeColorESL = Color.FromArgb(192, 0, 0);
        public static Color NodeColorETS = Color.Maroon;
        public static Color NodeColorITA = Color.FromArgb(0, 0, 192);
        public static Color NodeColorAEV = Color.FromArgb(0, 192, 0);
        public static Color NodeColorEXTRAS = Color.FromArgb(0x0062707E);
        public static Color NodeColorHided = Color.DarkGray;

        // color GL
        public static Vector4 GL_ColorESL = Utils.ColorToVector4(Color.Red);
        public static Vector4 GL_ColorETS = Utils.ColorToVector4(Color.Maroon);
        public static Vector4 GL_ColorITA = Utils.ColorToVector4(Color.Blue);
        public static Vector4 GL_ColorAEV = Utils.ColorToVector4(Color.Lime);
        public static Vector4 GL_ColorEXTRAS = Utils.ColorToVector4(Color.SlateGray);
        public static Vector4 GL_ColorSelected = Utils.ColorToVector4(Color.Yellow);
        public static Vector4 GL_ColorItemTriggerZone = Utils.ColorToVector4(Color.Fuchsia);
        public static Vector4 GL_ColorItemTriggerZoneSelected = Utils.ColorToVector4(Color.Pink);
        public static Vector4 GL_ColorItemTrigggerRadius = Utils.ColorToVector4(Color.DeepPink);
        public static Vector4 GL_ColorItemTrigggerRadiusSelected = Utils.ColorToVector4(Color.Plum);
        public static Vector4 GL_ColorGrid = Utils.ColorToVector4(Color.DarkGray);

        // more Colors
        public static Vector4 GL_MoreColor_T00_GeneralPurpose = Utils.ColorToVector4(Color.Green);
        public static Vector4 GL_MoreColor_T01_DoorWarp = Utils.ColorToVector4(Color.DarkOrange);
        public static Vector4 GL_MoreColor_T02_CutSceneEvents = Utils.ColorToVector4(Color.Olive);
        public static Vector4 GL_MoreColor_T04_GroupedEnemyTrigger = Utils.ColorToVector4(Color.Sienna);
        public static Vector4 GL_MoreColor_T05_Message = Utils.ColorToVector4(Color.MediumPurple);
        public static Vector4 GL_MoreColor_T08_TypeWriter = Utils.ColorToVector4(Color.Indigo);
        public static Vector4 GL_MoreColor_T0A_DamagesThePlayer = Utils.ColorToVector4(Color.LightSteelBlue);
        public static Vector4 GL_MoreColor_T0B_FalseCollision = Utils.ColorToVector4(Color.Crimson);
        public static Vector4 GL_MoreColor_T0D_Unknown = Utils.ColorToVector4(Color.DarkSeaGreen);
        public static Vector4 GL_MoreColor_T0E_Crouch = Utils.ColorToVector4(Color.BlanchedAlmond);
        public static Vector4 GL_MoreColor_T10_FixedLadderClimbUp = Utils.ColorToVector4(Color.SteelBlue);
        public static Vector4 GL_MoreColor_T11_ItemDependentEvents = Utils.ColorToVector4(Color.DarkViolet);
        public static Vector4 GL_MoreColor_T12_AshleyHideCommand = Utils.ColorToVector4(Color.Lavender);
        public static Vector4 GL_MoreColor_T13_LocalTeleportation = Utils.ColorToVector4(Color.DarkSalmon);
        public static Vector4 GL_MoreColor_T14_UsedForElevators = Utils.ColorToVector4(Color.YellowGreen);
        public static Vector4 GL_MoreColor_T15_AdaGrappleGun = Utils.ColorToVector4(Color.Navy);

        #endregion

        // backup da class config
        public static Re4QuadExtremeEditor.src.JSON.Configs BackupConfigs = null;

        #region Menu options
        public static bool RenderRoom = true;
        public static bool RenderEnemyESL = true;
        public static bool RenderEtcmodelETS = true;
        public static bool RenderItemsITA = true;
        public static bool RenderEventsAEV = true;

        public static bool RenderDisabledEnemy = true;
        public static bool RenderDontShowOnlyDefinedRoom = true;
        public static ushort RenderEnemyFromDefinedRoom = 0x0000;
        public static bool AutoDefinedRoom = false;

        public static bool RenderItemTriggerZone = true;
        public static bool RenderItemPositionAtAssociatedObjectLocation = false;
        public static bool RenderItemTriggerRadius = true;

        public static bool RenderSpecialTriggerZone = true;
        public static bool RenderExtraObjs = true;
        public static bool UseMoreSpecialColors = false;
        public static bool RenderExtraWarpDoor = true;
        public static bool HideExtraExceptWarpDoor = false;

        public static bool RenderEtcmodelUsingScale = false;
        public static bool TreeNodeRenderHexValues = false;
        public static bool PropertyGridUseHexFloat = false;
        public static bool SearchFilterMode = false;

        #endregion

        #region patch Files
        public static string FilePathESL = null;
        public static string FilePathETS = null;
        public static string FilePathITA = null;
        public static string FilePathAEV = null;
        #endregion

        // Render Options
        public static int FOV = 60;

        public static bool CreateEnemyExtraSegmentList = true;

        public static bool CamGridEnable = false;
        public static int CamGridvalue = 100;

        public static List<JSON.LangObjForList> Langs = new List<JSON.LangObjForList>();

        // treenode fonts
        public static Font TreeNodeFontText = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
        public static Font TreeNodeFontHex = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Bold);

        public static string OpenGLVersion = "";
        public static bool UseOldGL = false;

        #region Movement / Animation Settings (improved)

        /// <summary>
        /// Camera fly speed multiplier. Higher = faster movement.
        /// Default: 1.0 — increase for faster navigation.
        /// </summary>
        public static float CameraSpeedMultiplier = 1.5f;

        /// <summary>
        /// Camera mouse sensitivity multiplier.
        /// </summary>
        public static float CameraMouseSensitivity = 1.2f;

        /// <summary>
        /// Camera scroll wheel speed multiplier.
        /// </summary>
        public static float CameraScrollSpeed = 1.5f;

        /// <summary>
        /// Smooth camera movement interpolation factor (0.0 = instant, 1.0 = very smooth).
        /// </summary>
        public static float CameraSmoothFactor = 0.15f;

        /// <summary>
        /// Enable smooth camera interpolation (easing).
        /// </summary>
        public static bool CameraSmoothing = true;

        /// <summary>
        /// Timer interval in ms for WASD/movement updates. Lower = smoother.
        /// Default: 8ms (~120 fps update rate).
        /// </summary>
        public static int MovementTimerInterval = 8;

        /// <summary>
        /// Object move step size in world units.
        /// </summary>
        public static float ObjectMoveStep = 1.0f;

        #endregion

        #region Dark Mode

        /// <summary>
        /// Enable or disable Dark Mode for the UI.
        /// </summary>
        public static bool DarkModeEnabled = false;

        #endregion

        #region Platform Detection

        /// <summary>
        /// Returns true if running on macOS.
        /// </summary>
        public static bool IsMacOS =>
            System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                System.Runtime.InteropServices.OSPlatform.OSX);

        /// <summary>
        /// Returns true if running on Linux.
        /// </summary>
        public static bool IsLinux =>
            System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                System.Runtime.InteropServices.OSPlatform.Linux);

        /// <summary>
        /// Returns true if running on Windows.
        /// </summary>
        public static bool IsWindows =>
            System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                System.Runtime.InteropServices.OSPlatform.Windows);

        #endregion

    }
}
