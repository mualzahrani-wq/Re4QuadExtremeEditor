using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Re4QuadExtremeEditor.src.Class;
using Re4QuadExtremeEditor.src.Class.TreeNodeObj;
using Re4QuadExtremeEditor.src.Class.Enums;

namespace Re4QuadExtremeEditor.src
{
    /// <summary>
    /// Renders everything in the GL scene.
    /// Improvements: anti-aliasing, smooth lines, LOD support, high-quality depth.
    /// </summary>
    public static class TheRender
    {
        // ── High-Quality GL Initialization ───────────────────────────────────

        /// <summary>
        /// Call once in GlControl_Load after the base GL setup.
        /// Enables smooth lines, anti-aliasing hints, and better depth precision.
        /// </summary>
        public static void InitHighQualityGL()
        {
            // Smooth lines — eliminates jagged/aliased bounding box edges
            GL.Enable(EnableCap.LineSmooth);
            GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);

            // Smooth points
            GL.Enable(EnableCap.PointSmooth);
            GL.Hint(HintTarget.PointSmoothHint, HintMode.Nicest);

            // Perspective correction hint — better texture mapping quality
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            // Alpha blending — smooth transparent zones
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            // Tighter depth testing — reduces z-fighting at medium distances
            GL.DepthFunc(DepthFunction.Lequal);
            GL.ClearDepth(1.0f);

            // Crisp line width for bounding boxes (was 1.5f)
            GL.LineWidth(2.0f);

            // Larger point size for vertex indicators
            GL.PointSize(4.0f);
        }

        /// <summary>
        /// Current camera world position. Set each frame in Render().
        /// Used for LOD and distance-based quality decisions.
        /// </summary>
        public static Vector3 CameraPosition = Vector3.Zero;

        // ── LOD (Level of Detail) ─────────────────────────────────────────────
        /// <summary>
        /// If > 0, objects beyond this distance skip 3D model and show bounding box only.
        /// Set to 0 to DISABLE LOD (all models always visible — fixes "half body" issue).
        /// </summary>
        public static float LOD_MaxModelDistance = 0f; // 0 = disabled

        private static float DistanceSq(Vector3 a, Vector3 b)
        {
            float dx = a.X - b.X, dy = a.Y - b.Y, dz = a.Z - b.Z;
            return dx * dx + dy * dy + dz * dz;
        }

        #region info
        /*
        Para rederização Normal usar:
        TheRender.Render(ref camMtx, ref ProjMatrix, float);

        Para seleção de objeto usa-se a renderização "ToSelect":
        TheRender.RenderToSelect(ref camMtx, ref ProjMatrix);

        -------
        Referente a escala das coordenadas:
        Toda a escala do que esta sendo renderizado é 100 vezes menor que a escala de coordenadas dos arquivos AEV, ITA e ETS;

        Sendo para os arquivos PMD:
        * pmds de itens, inimigos e etcmodel, estão na scala 1 pra 1 no oque esta sendo renderizado,
        * os pmds dos cenarios são 10 vezes menor que os pmds citados acima.

        Para as coordenas do ESL:
        * os valores shorts das coordenadas devem ser multiplicado por 10 para estar na mesma escala que o AEV.

        Para outros arquivos:
        * caso na edição de novos arquivos, dividir as coordenadas por 100, pois estão na mesma escala que o AEV.

        -----
        Referente a renderização para a seleção:
        Todo o fundo é renderizado na cor branca (white) int: 0xFFFFFFFF
        Todo o model 3d do cenario (Room) é renderizado na cor preta (black) int: 0x000000FF

        Todos os objtos são rederizados por cores, nas quais são definidas pelo id e grupo do objeto, Sendo

        O primeiro e segundo byte da cor o ID (lineID) do objeto
        E o terceiro byte o Grupo do objeto
        E o quarto byte é 0xFF pois tem que ser uma cor solida.

        Nota: no openGL as cores vão de 0 a 1, então todos os valores devem ser divididos por 255;

        Os numeros dos grupos não podem ser 0 e 255, os outros valores podem ser usado, sendo os ja usados:
        1 = ESL
        2 = ETS
        3 = ITA
        4 = AEV
        5 = EXTRAS

        */
        #endregion

        private static readonly Vector3 boundOff = new Vector3(1f, 1f, 1f);
        private static readonly Vector3 boundNoneEnemy = new Vector3(3f, 4f, 3f);
        private static readonly Vector3 boundNoneEtcModel = new Vector3(3f, 3f, 3f);
        private static readonly Vector3 boundNoneItem = new Vector3(1.5f, 1.5f, 1.5f);
        private static readonly Vector3 boundNoneExtras = new Vector3(2f, 2f, 2f);

        #region normal Render

        private static void drawGrid(float objY)
        {
            DataBase.ShaderBoundingBox.Use();
            DataBase.ShaderBoundingBox.SetVector3("mScale", Vector3.One);
            DataBase.ShaderBoundingBox.SetMatrix4("mRotation", Matrix4.Identity);
            DataBase.ShaderBoundingBox.SetAltRotation(OldRotation.Identity);
            DataBase.ShaderBoundingBox.SetVector3("mPosition", new Vector3(0, objY, 0));
            DataBase.ShaderBoundingBox.SetVector4("mColor", Globals.GL_ColorGrid);
            DataBase.ShaderBoundingBox.Start();
            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.Texture2D);
            GL.Disable(EnableCap.AlphaTest);
            if (Globals.CamGridvalue != 0)
            {
                float spaceBetweenTheLines = Globals.CamGridvalue / 10f;
                int lenght = 6560;
                int numberLines = (int)(lenght / spaceBetweenTheLines);
                int numberLines2 = numberLines / 2;
                float endPoint = (numberLines2 * spaceBetweenTheLines);
                float positionX = 0;
                float positionZ = 0;
                GL.PushMatrix();
                GL.Begin(PrimitiveType.Lines);
                for (int i = 0; i <= numberLines2; i++)
                {
                    GL.Vertex3(positionX, 0, -endPoint);
                    GL.Vertex3(positionX, 0, endPoint);
                    GL.Vertex3(-endPoint, 0, positionZ);
                    GL.Vertex3(endPoint, 0, positionZ);
                    positionX -= spaceBetweenTheLines;
                    positionZ -= spaceBetweenTheLines;
                }
                positionX = 0; positionZ = 0;
                for (int i = 0; i <= numberLines2; i++)
                {
                    GL.Vertex3(positionX, 0, -endPoint);
                    GL.Vertex3(positionX, 0, endPoint);
                    GL.Vertex3(-endPoint, 0, positionZ);
                    GL.Vertex3(endPoint, 0, positionZ);
                    positionX += spaceBetweenTheLines;
                    positionZ += spaceBetweenTheLines;
                }
                GL.End();
                GL.PopMatrix();
            }
            GL.Enable(EnableCap.Texture2D);
        }

        public static void Render(ref Matrix4 camMtx, ref Matrix4 ProjMatrix, float objY)
        {
            // Extract camera position from view matrix for LOD calculations
            // View matrix inverse gives camera world position
            Matrix4 invView = Matrix4.Invert(camMtx);
            CameraPosition = invView.Row3.Xyz;

            GL.ClearColor(Globals.SkyColor);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Re-enable quality settings each frame (some states may be reset by drivers)
            GL.Enable(EnableCap.LineSmooth);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            if (Globals.RenderRoom && DataBase.SelectedRoom != null)
            {
                DataBase.ShaderRoom.Use();
                DataBase.ShaderRoom.SetMatrix4("view", camMtx);
                DataBase.ShaderRoom.SetMatrix4("projection", ProjMatrix);
                DataBase.ShaderRoom.Start();
                DataBase.SelectedRoom.Render();
            }

            DataBase.ShaderObjs.Use();
            DataBase.ShaderObjs.SetMatrix4("view", camMtx);
            DataBase.ShaderObjs.SetMatrix4("projection", ProjMatrix);
            DataBase.ShaderBoundingBox.Use();
            DataBase.ShaderBoundingBox.SetMatrix4("view", camMtx);
            DataBase.ShaderBoundingBox.SetMatrix4("projection", ProjMatrix);

            if (Globals.RenderEnemyESL) RenderEnemyESL();
            if (Globals.RenderExtraObjs) RenderExtras();
            if (Globals.RenderEventsAEV) RenderAEV();
            if (Globals.RenderItemsITA) RenderITA();
            if (Globals.RenderEtcmodelETS) RenderEtcModelETS();
            if (Globals.CamGridEnable) drawGrid(objY);

            RenderPosTriggerZoneBox();
            GL.Finish();
        }
