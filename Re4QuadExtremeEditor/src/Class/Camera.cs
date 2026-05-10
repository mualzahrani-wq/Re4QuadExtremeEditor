using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using Re4QuadExtremeEditor.src.Class.TreeNodeObj;

namespace Re4QuadExtremeEditor.src.Class
// basedo em https://github.com/DavidSM64/Quad64/blob/master/src/Viewer/Camera.cs
// e em https://github.com/opentk/LearnOpenTK/blob/master/Common/Camera.cs
{

public class Camera
{
public enum CameraMode
{
FLY = 0,
ORBIT = 1,
LOOK_DIRECTION = 2
}

public enum LookDirection
{
TOP,
BOTTOM,
LEFT,
RIGHT,
FRONT,
BACK
}

private readonly Vector3[] lookPositions = new Vector3[]
{//3280
new Vector3(0, 1640, 0), // top
new Vector3(0, -1640, 0), // bottom
new Vector3(-1640, 0, 0), // left
new Vector3(1640, 0, 0), // right
new Vector3(0, 0, 1640), // front
new Vector3(0, 0, -1640) // back
};

private readonly Vector2[] lookEye = new Vector2[] // _pitch, _yaw, in degrees
{
new Vector2(0, -180), // top
new Vector2(0, 180), // bottom
new Vector2(0, 0), // left
new Vector2(180, 0), // right
new Vector2(-90, 0), // front
new Vector2(90, 0) // back
};

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

public Vector3 Position { get { return pos; } set { pos = value; } }
public Vector3 LookAt { get { return lookat; } set { lookat = value; } }
public Vector3 SavedCamPos { get { return savedCamPos; } set { savedCamPos = value; } }

// Smooth movement - velocity vectors for fluid camera motion
private Vector3 _velocity = Vector3.Zero;
private float _velocityDecay = 0.82f; // كلما كان أقل كلما توقفت أسرع، أكبر = أكثر سلاسة
private const float _velocityThreshold = 0.0001f;

// Smooth mouse rotation
private float _targetYaw;
private float _targetPitch;
private float _rotationSmoothing = 0.25f; // سرعة تتبع الماوس (0=بطيء جداً، 1=فوري)

private Vector3 _front = -Vector3.UnitZ;
private Vector3 _up = Vector3.UnitY;
private Vector3 _right = Vector3.UnitX;

public Vector3 Front => _front;
public Vector3 Up => _up;
public Vector3 Right => _right;

private float _pitch;
private float _yaw = -MathHelper.PiOver2;

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

public void resetMouseStuff()
{
resetMouse = true;
}

public void SaveCameraPosition()
{
savedCamPos = pos;
}

private float orbitDistance = 500.0f;
private float orbitTheta = 0.0f, orbitPhi = 0.0f;

public bool isOrbitCamera()
{
return camMode == CameraMode.ORBIT;
}

private Vector3 moveObj_front = -Vector3.UnitZ;
private Vector3 moveObj_right = Vector3.UnitX;

public Vector3 moveObjFront => moveObj_front;
public Vector3 moveObjRight => moveObj_right;

public Camera()
{
_targetYaw = _yaw;
_targetPitch = _pitch;
UpdateVectors();
orientateCam();
}

public void ResetCameraToZero()
{
camMode = CameraMode.FLY;
pos = Vector3.Zero;
_pitch = 0;
_yaw = -MathHelper.PiOver2;
_targetYaw = _yaw;
_targetPitch = _pitch;
_velocity = Vector3.Zero;
UpdateVectors();
orientateCam();
resetMouse = true;
}

public void SetToFlyMode()
{
camMode = CameraMode.FLY;
_velocity = Vector3.Zero;
UpdateVectors();
orientateCam();
resetMouse = true;
}

public void SetToOrbitMode()
{
camMode = CameraMode.ORBIT;
_velocity = Vector3.Zero;
resetOrbitToSelectedObject();
updateOrbitCamera();
UpdateVectors();
resetMouse = true;
}

public void setCameraMode_LookDirection(LookDirection dir)
{
camMode = CameraMode.LOOK_DIRECTION;
currentLookDirection = dir;
pos = lookPositions[(int)dir];
YawDegrees = lookEye[(int)dir].X;
PitchDegrees = lookEye[(int)dir].Y;
_velocity = Vector3.Zero;
orientateCam();
resetMouse = true;
}

public Matrix4 GetViewMatrix()
{
if (camMode == CameraMode.ORBIT && getSelectedObject() != null)
{
return Matrix4.LookAt(pos, lookat, FixedUp);
}
return Matrix4.LookAt(pos, pos + _front, _up);
}

// تحديث الحركة السلسة - يستدعى كل فريم
public void UpdateSmoothMovement()
{
if (_velocity.LengthSquared > _velocityThreshold)
{
pos += _velocity;
_velocity *= _velocityDecay;
}
else
{
_velocity = Vector3.Zero;
}

// Smooth rotation interpolation
if (camMode == CameraMode.FLY)
{
float pitchDiff = _targetPitch - _pitch;
float yawDiff = _targetYaw - _yaw;
if (Math.Abs(pitchDiff) > 0.0001f || Math.Abs(yawDiff) > 0.0001f)
{
_pitch += pitchDiff * _rotationSmoothing;
_yaw += yawDiff * _rotationSmoothing;
UpdateVectors();
}
}
}

private void UpdateAnglesFromOrbit()
{
Vector3 direction = Vector3.Normalize((lookat - pos)/100);
_yaw = (float)Math.Atan2(direction.Z, direction.X);
_pitch = (float)Math.Asin(direction.Y);
_targetYaw = _yaw;
_targetPitch = _pitch;
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

public void updateCameraToUp()
{
_velocity += _up * camSpeedMultiplier * 0.3f;
}

public void updateCameraToDown()
{
_velocity -= _up * camSpeedMultiplier * 0.3f;
}

public void updateCameraToRight()
{
_velocity += _right * camSpeedMultiplier * 0.3f;
}

public void updateCameraToLeft()
{
_velocity -= _right * camSpeedMultiplier * 0.3f;
}

public void updateCameraToFront()
{
_velocity += _front * camSpeedMultiplier * 0.3f;
}

public void updateCameraToBack()
{
_velocity -= _front * camSpeedMultiplier * 0.3f;
}

public void updateCameraOffsetMatrixWithMouse(bool isControlDown, int mouseX, int mouseY, bool invert = false)
{
if (camMode == CameraMode.ORBIT && getSelectedObject() != null)
{
updateCameraOffsetWithMouse_ORBIT(mouseX, mouseY, invert);
}
else if (camMode == CameraMode.LOOK_DIRECTION || isControlDown)
{
updateCameraOffsetWithMouse_LOOK(mouseX, mouseY, invert, isControlDown);
}
else
{
updateCameraMatrixWithMouse_FLY(mouseX, mouseY, invert);
}
}

private void updateCameraMatrixWithMouse_FLY(int mouseX, int mouseY, bool invert)
{
if (resetMouse)
{
lastMouseX = mouseX;
lastMouseY = mouseY;
resetMouse = false;
return;
}

int MousePosX = mouseX - lastMouseX;
int MousePosY = mouseY - lastMouseY;
lastMouseX = mouseX;
lastMouseY = mouseY;

if (invert)
{
MousePosX = -MousePosX;
MousePosY = -MousePosY;
}

const float sensitivity = 0.2f;
_targetYaw = _yaw + MathHelper.DegreesToRadians(MousePosX * sensitivity);
float newPitchDeg = MathHelper.RadiansToDegrees(_targetPitch) - MousePosY * sensitivity;
newPitchDeg = MathHelper.Clamp(newPitchDeg, -89f, 89f);
_targetPitch = MathHelper.DegreesToRadians(newPitchDeg);
}

private void updateCameraOffsetWithMouse_LOOK(int mouseX, int mouseY, bool invert, bool isControlDown)
{
if (resetMouse)
{
lastMouseX = mouseX;
lastMouseY = mouseY;
resetMouse = false;
}
int MousePosX = (-mouseX) + lastMouseX;
int MousePosY = (-mouseY) + lastMouseY;

float extraSpeed = 1f;
if (camMode == CameraMode.LOOK_DIRECTION && !isControlDown)
{
extraSpeed = 4f;
}

if (invert)
{
MousePosX = -MousePosX;
MousePosY = -MousePosY;
}

float sensitivity = 0.2f * extraSpeed * camSpeedMultiplier;
pos = savedCamPos + (-_right * (MousePosX) * sensitivity) + (_up * (MousePosY) * sensitivity);
}

private void updateCameraOffsetWithMouse_ORBIT(int mouseX, int mouseY, bool invert)
{
if (resetMouse)
{
lastMouseX = mouseX;
lastMouseY = mouseY;
resetMouse = false;
}
int MousePosX = (-mouseX) + lastMouseX;
int MousePosY = (-mouseY) + lastMouseY;
lastMouseX = mouseX;
lastMouseY = mouseY;
if (invert)
{
MousePosX = -MousePosX;
MousePosY = -MousePosY;
}
orbitTheta += MousePosX * 0.01f * camSpeedMultiplier;
orbitPhi -= MousePosY * 0.01f * camSpeedMultiplier;
orbitPhi = MathHelper.Clamp(orbitPhi, -1.57f, 1.57f);
updateOrbitCamera();
}

public void updateCameraMatrixWithScrollWheel(int amt, bool invert = false)
{
if (invert) { amt = -amt; }
if (camMode == CameraMode.ORBIT && getSelectedObject() != null)
{
updateCameraMatrixWithScrollWheel_ORBIT(amt);
}
else
{
updateCameraMatrixWithScrollWheel_FLY(amt);
}
}

private void updateCameraMatrixWithScrollWheel_FLY(int amt)
{
_velocity += _front * amt * camSpeedMultiplier * 0.5f;
}

private void updateCameraMatrixWithScrollWheel_ORBIT(int amt)
{
orbitDistance -= amt * 0.2f * camSpeedMultiplier;
if (orbitDistance < 10.0f) { orbitDistance = 10.0f; }
updateOrbitCamera();
}

private Object3D getSelectedObject()
{
if (DataBase.LastSelectNode is Object3D node) { return node; }
return null;
}

public float SelectedObjPosY()
{
Object3D obj = getSelectedObject();
if (obj == null) { return 0; }
return obj.GetObjPosition_ToCamera().Y;
}

private void updateOrbitCamera()
{
if (camMode == CameraMode.ORBIT)
{
Object3D obj = getSelectedObject();
if (obj == null) { return; }
Vector3 objPos = obj.GetObjPosition_ToCamera();
pos.X = objPos.X + (float)(Math.Cos(orbitPhi) * -Math.Sin(orbitTheta) * orbitDistance);
pos.Y = objPos.Y + (float)(-Math.Sin(orbitPhi) * orbitDistance);
pos.Z = objPos.Z + (float)(Math.Cos(orbitPhi) * Math.Cos(orbitTheta) * orbitDistance);
lookat.X = objPos.X;
lookat.Y = objPos.Y;
lookat.Z = objPos.Z;
UpdateAnglesFromOrbit();
}
}

public void resetOrbitToSelectedObject()
{
Object3D obj = getSelectedObject();
if (obj != null)
{
orbitTheta = -obj.GetObjAngleY_ToCamera();
orbitPhi = -0.30f;
orbitDistance = 100.0f;
}
}

public void UpdateCameraOrbitOnChangeObj()
{
if (camMode == CameraMode.ORBIT && getSelectedObject() != null)
{
updateOrbitCamera();
UpdateVectors();
resetMouse = true;
}
}

public void UpdateCameraOrbitOnChangeValue()
{
if (camMode == CameraMode.ORBIT && getSelectedObject() != null)
{
updateOrbitCamera();
UpdateVectors();
resetMouse = true;
}
}

}
}
