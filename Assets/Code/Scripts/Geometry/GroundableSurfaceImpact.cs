using UnityEngine;

public class GroundableSurfaceImpact {
  private int _surfaceInstanceId;
  private MyTransform _impactTransform;
  private Vector3 _impactPoint;
  private Collider _impactCollider;

  public GroundableSurfaceImpact(int instanceId, Vector3 position, Vector3 rotation, Vector3 scale, Vector3 impactPosition, Collider impactCollider) {
    _surfaceInstanceId = instanceId;
    _impactTransform = new MyTransform(position, rotation, scale);
    _impactPoint = impactPosition;
    _impactCollider = impactCollider;
  }

  public int GetInstanceId() { return _surfaceInstanceId; }

  public Vector3 GetPosition() { return _impactTransform._position; }
  public Vector3 GetRotation() { return _impactTransform._rotation; } // if euler angle value is negative, will show up as 360 - negative value.
  public Vector3 GetScale() { return _impactTransform._scale; }

  public Vector3 GetImpactPosition() { return _impactPoint; }

  public Collider GetImpactCollider() { return _impactCollider; }

  private class MyTransform {
    public Vector3 _position;
    public Vector3 _rotation;
    public Vector3 _scale;

    public MyTransform(Vector3 position, Vector3 rotation, Vector3 scale) {
      _position = position;
      _rotation = rotation;
      _scale = scale;
    }
  }
}