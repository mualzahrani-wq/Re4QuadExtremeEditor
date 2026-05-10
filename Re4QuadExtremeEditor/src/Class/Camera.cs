using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using Re4QuadExtremeEditor.src.Class.TreeNodeObj;

namespace Re4QuadExtremeEditor.src.Class
{
    public class Camera
    {
        public enum CameraMode : int { FLY = 0, ORBIT = 1, LOOK_DIRECTION = 2 }
        public enum LookDirection : int { TOP = 0, BOTTOM = 1, LEFT = 2, RIGHT = 3, FRONT = 4, BACK = 5 }

        private readonly Vector3[] lookPositions = new Vector3[]
        {
            new Vector3(0, 1640, 0), new Vector3(0, -1640, 0),
            new Vector3(-1640, 0, 0), new Vector3(1640, 0, 0),
            new Vector3(0, 0, 1640), new Vector3(0, 0, -1640)
        };

        private readonly Vector2[] lookEye = new Vector2[]
        {
            new Vector2(0, -180), new Vector2(0, 180),
            new Vector2(0, 0), new Vector2(180, 0),
            new Vector2(-90, 0), new Vector2(90, 0)
        };

        private float camExtraSpeed = 3.0f;
        public float CamExtraSpeed { get { return camExtraSpeed; } set { camExtraSpeed = value; } }

        private float camSpeedMultiplier = 1.0f;
        public float CamSpeedMultiplier { get { return camSpeedMultiplier; } set { camSpeedMultiplier = value; } }

        private readonly Vector3 FixedUp = new Vector3(0, 1, 0);
        private CameraMode camMode = CameraMode.FLY;
        private LookDirection currentLookDirection;

        public CameraMode CamMode { get { return camMode; } }
        public LookDirection CurrentLookDirection { get { return currentLookDirection; } }

        private Vector3 pos = Vector3.Zero;
        private Vector3 lookat = Vector3.Zero;
        private Vector3 savedCamPos = Vector3.Zero;

        // ── Smooth movement interpolation ────────────────────────────────────
        private Vector3 _smoothVelocity = Vector3.Zero;
        private const float SmoothDecay = 0.82f; // higher = more glide (0.0 = instant stop)

        public Vector3 Position { get { return pos; } set { pos = value; } }
        public Vector3 LookAt { get { return lookat; } set { lookat = value; } }
        public Vector3 SavedCamPos { get { return savedCamPos; } set { savedCamPos = value; } }

        private Vector3 _front = -Vector3.UnitZ;
        private Vector3 _up = Vector3.UnitY;
        private Vector3 _right = Vector3.UnitX;

        public Vector3 Front => _front;
        public Vector3 Up => _up;
        public Vector3 Right => _right;

        private float _pitch;
        private float _yaw = -MathHelper.PiOver2;
        private float _targetYaw = -MathHelper.PiOver2;
        private float _targetPitch = 0f;

        public float Yaw { get { return _yaw; } }
        public float Pitch { get { return _pitch; } }

        public float PitchDegrees
        {
            get => MathHelper.RadiansToDegrees(_pitch);
            set
            {
                var angle = MathHelper.Clamp(value, -89f, 89f);
                _pitch = MathHelper.DegreesToRadians(angle);
                _targetPitch = _pitch;
                UpdateVectors();
            }
        }

        public float YawDegrees
        {
            get => MathHelper.RadiansToDegrees(_yaw);
            set
            {
                _yaw = MathHelper.DegreesToRadians(value);
                _targetYaw = _yaw;
                UpdateVectors();
            }
        }

        private bool resetMouse = true;
        private int lastMouseX = -1, lastMouseY = -1;

        public void resetMouseStuff() { resetMouse = true; }
        public void SaveCameraPosition() { savedCamPos = pos; }

        private float orbitDistance = 500.0f;
        private float orbitTheta = 0.0f, orbitPhi = 0.0f;
        public bool isOrbitCamera() { return camMode == CameraMode.ORBIT; }

        private Vector3 moveObj_front = -Vector3.UnitZ;
        private Vector3 moveObj_right = Vector3.UnitX;
        public Vector3 MoveObjFront => moveObj_front;
        public Vector3 MoveObjRight => moveObj_right;

        public Camera() { UpdateVectors(); orientateCam(); }

        public void ResetCameraToZero()
        {
            camMode = CameraMode.FLY;
            pos = Vector3.Zero;
            _pitch = 0; _targetPitch = 0;
            _yaw = -MathHelper.PiOver2; _targetYaw = _yaw;
            _smoothVelocity = Vector3.Zero;
            UpdateVectors(); orientateCam(); resetMouse = true;
        }

        public void SetToFlyMode() { camMode = CameraMode.FLY; UpdateVectors(); orientateCam(); resetMouse = true; }

        public void SetToOrbitMode()
        {
            camMode = CameraMode.ORBIT;
            ResetOrbitToSelectedObject();
            updateOrbitCamera(); UpdateVectors(); resetMouse = true;
        }

        public void setCameraMode_LookDirection(LookDirection dir)
        {
            camMode = CameraMode.LOOK_DIRECTION;
            currentLookDirection = dir;
            pos = lookPositions[(int)dir];
            YawDegrees = lookEye[(int)dir].X;
            PitchDegrees = lookEye[(int)dir].Y;
            orientateCam(); resetMouse = true;
        }

        public Matrix4 GetViewMatrix()
        {
            if (camMode == CameraMode.ORBIT && getSelectedObject != null && getSelectedObject() != null)
                return Matrix4.LookAt(pos, lookat, FixedUp);
            return Matrix4.LookAt(pos, pos + _front, _up);
        }

        private void UpdateAnglesFromOrbit()
        {
            Vector3 direction = Vector3.Normalize((lookat - pos) / 100f);
            _yaw = (float)Math.Atan2(direction.Z, direction.X);
            _pitch = (float)Math.Asin(direction.Y);
            _targetYaw = _yaw; _targetPitch = _pitch;
            UpdateVectors();
        }

        private void UpdateVectors()
        {
            _front.X = (float)Math.Cos(_pitch) * (float)Math.Cos(_yaw);
            _front.Y = (float)Math.Sin(_pitch);
            _front.Z = (float)Math.Cos(_pitch) * (float)Math.Sin(_yaw);
            _front = Vector3.Normalize(_front);
            _right = Vector3.Normalize(Vector3.Cross(_front, Vector3.UnitY));
            _up = Vector3.Normalize(Vector3.Cross(_right, _front));
            moveObj_front.X = (float)Math.Cos(_yaw);
            moveObj_front.Y = 0;
            moveObj_front.Z = (float)Math.Sin(_yaw);
            moveObj_front = Vector3.Normalize(moveObj_front);
            moveObj_right = Vector3.Normalize(Vector3.Cross(_front, Vector3.UnitY));
        }

        private void orientateCam()
        {
            float CamLX = (float)Math.Cos(_pitch) * (float)Math.Cos(_yaw);
            float CamLY = (float)Math.Sin(_pitch);
            float CamLZ = (float)Math.Cos(_pitch) * (float)Math.Sin(_yaw);
            lookat.X = (pos.X + CamLX) * 100f;
            lookat.Y = (pos.Y + CamLY) * 100f;
            lookat.Z = (pos.Z + CamLZ) * 100f;
        }

        // ── Movement with smooth velocity ──────────────────────────────────

        private void MoveWithSmoothing(Vector3 direction)
        {
            float speed = camSpeedMultiplier * camExtraSpeed * Globals.CameraSpeedMultiplier;
            if (Globals.CameraSmoothing)
            {
                _smoothVelocity += direction * speed * 0.35f;
                // clamp velocity
                float maxSpeed = speed * 2.5f;
                if (_smoothVelocity.Length > maxSpeed)
                    _smoothVelocity = Vector3.Normalize(_smoothVelocity) * maxSpeed;
                pos += _smoothVelocity;
            }
            else
            {
                pos += direction * speed;
            }
        }

        /// <summary>
        /// Call every frame to decay smooth velocity (call from timer tick).
        /// </summary>
        public void ApplySmoothDecay()
        {
            if (Globals.CameraSmoothing)
                _smoothVelocity *= SmoothDecay;
        }

        public void updateCameraToUp()    { MoveWithSmoothing(_up); }
        public void updateCameraToDown()  { MoveWithSmoothing(-_up); }
        public void updateCameraToRight() { MoveWithSmoothing(_right); }
        public void updateCameraToLeft()  { MoveWithSmoothing(-_right); }
        public void updateCameraToFront() { MoveWithSmoothing(_front); }
        public void updateCameraToBack()  { MoveWithSmoothing(-_front); }

        // ── Mouse look with smooth rotation ──────────────────────────────

        public void updateCameraOffsetMatrixWithMouse(bool isControlDown, int mouseX, int mouseY, bool invert = false)
        {
            if (camMode == CameraMode.ORBIT && getSelectedObject != null && getSelectedObject() != null)
                updateCameraOffsetWithMouse_ORBIT(mouseX, mouseY, invert);
            else if (camMode == CameraMode.LOOK_DIRECTION || isControlDown)
                updateCameraOffsetWithMouse_LOOK(mouseX, mouseY, invert, isControlDown);
            else
                updateCameraMatrixWithMouse_FLY(mouseX, mouseY, invert);
        }

        private void updateCameraMatrixWithMouse_FLY(int mouseX, int mouseY, bool invert)
        {
            if (resetMouse) { lastMouseX = mouseX; lastMouseY = mouseY; resetMouse = false; return; }

            int dx = mouseX - lastMouseX;
            int dy = mouseY - lastMouseY;
            lastMouseX = mouseX; lastMouseY = mouseY;

            if (invert) { dx = -dx; dy = -dy; }

            // Dynamic sensitivity — slightly higher for smooth feel
            float sensitivity = 0.18f * Globals.CameraMouseSensitivity;
            _targetYaw += MathHelper.DegreesToRadians(dx * sensitivity);
            _targetPitch -= MathHelper.DegreesToRadians(dy * sensitivity);
            _targetPitch = MathHelper.Clamp(_targetPitch, MathHelper.DegreesToRadians(-89f), MathHelper.DegreesToRadians(89f));

            // Smooth interpolation
            float lerpFactor = Globals.CameraSmoothing ? 0.55f : 1.0f;
            _yaw += (_targetYaw - _yaw) * lerpFactor;
            _pitch += (_targetPitch - _pitch) * lerpFactor;
            UpdateVectors();
        }

        private void updateCameraOffsetWithMouse_LOOK(int mouseX, int mouseY, bool invert, bool isControlDown)
        {
            if (resetMouse) { lastMouseX = mouseX; lastMouseY = mouseY; resetMouse = false; }
            int MousePosX = (-mouseX) + lastMouseX;
            int MousePosY = (-mouseY) + lastMouseY;
            float controlDownExtraSpeed = 1f;
            if (camMode == CameraMode.LOOK_DIRECTION && !isControlDown) controlDownExtraSpeed = 4f;
            if (invert) { MousePosX = -MousePosX; MousePosY = -MousePosY; }
            float sensitivity = 0.2f * controlDownExtraSpeed * (camSpeedMultiplier * camExtraSpeed) * Globals.CameraMouseSensitivity;
            pos = savedCamPos + (-_right * MousePosX * sensitivity) + (_up * MousePosY * sensitivity);
        }

        private void updateCameraOffsetWithMouse_ORBIT(int mouseX, int mouseY, bool invert)
        {
            if (resetMouse) { lastMouseX = mouseX; lastMouseY = mouseY; resetMouse = false; return; }
            int MousePosX = (-mouseX) + lastMouseX;
            int MousePosY = (-mouseY) + lastMouseY;
            lastMouseX = mouseX; lastMouseY = mouseY;
            if (invert) { MousePosX = -MousePosX; MousePosY = -MousePosY; }
            orbitTheta += MousePosX * 0.01f * camSpeedMultiplier * Globals.CameraMouseSensitivity;
            orbitPhi -= MousePosY * 0.01f * camSpeedMultiplier * Globals.CameraMouseSensitivity;
            orbitPhi = MathHelper.Clamp(orbitPhi, -1.57f, 1.57f);
            updateOrbitCamera();
        }

        // ── Scroll wheel with smooth speed ────────────────────────────────

        public void updateCameraMatrixWithScrollWheel(int amt, bool invert = false)
        {
            if (invert) amt = -amt;
            if (camMode == CameraMode.ORBIT && getSelectedObject != null && getSelectedObject() != null)
                updateCameraMatrixWithScrollWheel_ORBIT(amt);
            else
                updateCameraMatrixWithScrollWheel_FLY(amt);
        }

        private void updateCameraMatrixWithScrollWheel_FLY(int amt)
        {
            float scrollSpeed = camSpeedMultiplier * Globals.CameraScrollSpeed;
            if (Globals.CameraSmoothing)
            {
                _smoothVelocity += _front * amt * scrollSpeed * 0.4f;
                float maxS = scrollSpeed * 8f;
                if (_smoothVelocity.Length > maxS)
                    _smoothVelocity = Vector3.Normalize(_smoothVelocity) * maxS;
            }
            else
            {
                pos += _front * amt * scrollSpeed;
            }
        }

        private void updateCameraMatrixWithScrollWheel_ORBIT(int amt)
        {
            orbitDistance -= amt * 0.2f * camSpeedMultiplier * Globals.CameraScrollSpeed;
            if (orbitDistance < 10.0f) orbitDistance = 10.0f;
            updateOrbitCamera();
        }

        public Func<IObject3D> getSelectedObject;

        public float SelectedObjPosY()
        {
            if (getSelectedObject == null) return 0;
            IObject3D obj = getSelectedObject();
            if (obj == null) return 0;
            return obj.GetObjPosition_ToCamera().Y;
        }

        private void updateOrbitCamera()
        {
            if (camMode == CameraMode.ORBIT)
            {
                if (getSelectedObject == null) return;
                IObject3D obj = getSelectedObject();
                if (obj == null) return;
                Vector3 objPos = obj.GetObjPosition_ToCamera();
                pos.X = objPos.X + (float)(Math.Cos(orbitPhi) * -Math.Sin(orbitTheta) * orbitDistance);
                pos.Y = objPos.Y + (float)(-Math.Sin(orbitPhi) * orbitDistance);
                pos.Z = objPos.Z + (float)(Math.Cos(orbitPhi) * Math.Cos(orbitTheta) * orbitDistance);
                lookat.X = objPos.X; lookat.Y = objPos.Y; lookat.Z = objPos.Z;
                UpdateAnglesFromOrbit();
            }
        }

        public void ResetOrbitToSelectedObject()
        {
            if (getSelectedObject == null) return;
            IObject3D obj = getSelectedObject();
            if (obj != null)
            {
                orbitTheta = -obj.GetObjAngleY_ToCamera();
                orbitPhi = -0.30f;
                orbitDistance = 100.0f;
            }
        }

        public void UpdateCameraOrbitOnChangeObj()
        {
            if (camMode == CameraMode.ORBIT && getSelectedObject != null && getSelectedObject() != null)
            { updateOrbitCamera(); UpdateVectors(); resetMouse = true; }
        }

        public void UpdateCameraOrbitOnChangeValue()
        {
            if (camMode == CameraMode.ORBIT && getSelectedObject != null && getSelectedObject() != null)
            { updateOrbitCamera(); UpdateVectors(); resetMouse = true; }
        }
    }

    public interface IObject3D
    {
        Vector3 GetObjPosition_ToCamera();
        float GetObjAngleY_ToCamera();
    }
}
