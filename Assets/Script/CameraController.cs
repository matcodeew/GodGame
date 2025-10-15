using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float ZoomSpeed;
    public float MinZoom;
    public float MaxZoom;
    public float MinX, MaxX, MinZ, MaxZ;

    private float _maxSpeed = 50f;
    private float _accelerationRate = 20f;
    private float _speed = 30f;
    private float _border = 50.0f; // La zone de la bordure où la caméra commence à se déplacer
    private float _accelerationX = 5f;
    private float _accelerationZ = 5f;

    private Camera _cam;

    private void Start()
    {
        _cam = GetComponent<Camera>();
    }

    void Update()
    {
        MoveCamera();
        HandleZoom();
    }

    private void HandleZoom()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        if (_cam.orthographic)
        {
            _cam.orthographicSize -= scrollInput * ZoomSpeed;
            _cam.orthographicSize = Mathf.Clamp(_cam.orthographicSize, MinZoom, MaxZoom);
        }
        else
        {
            _cam.fieldOfView -= scrollInput * ZoomSpeed;
            _cam.fieldOfView = Mathf.Clamp(_cam.fieldOfView, MinZoom, MaxZoom);
        }
    }

    private void MoveCamera()
    {
        if (_cam != null)
        {
            Vector3 newPosition = _cam.transform.position;

            // Vérification du bord droit (mouvement à droite)
            if (Input.mousePosition.x >= Screen.width - _border)
            {
                _accelerationX += _accelerationRate * Time.deltaTime;
                _accelerationX = Mathf.Min(_accelerationX, _maxSpeed);

                newPosition.x += _accelerationX * Time.deltaTime;
            }
            // Vérification du bord gauche (mouvement à gauche)
            else if (Input.mousePosition.x <= _border)
            {
                _accelerationX += _accelerationRate * Time.deltaTime;
                _accelerationX = Mathf.Min(_accelerationX, _maxSpeed);

                newPosition.x -= _accelerationX * Time.deltaTime;
            }
            else
            {
                _accelerationX = 0f;
            }

            // Vérification du bord du haut (mouvement vers l'avant sur l'axe Z)
            if (Input.mousePosition.y >= Screen.height - _border)
            {
                _accelerationZ += _accelerationRate * Time.deltaTime;
                _accelerationZ = Mathf.Min(_accelerationZ, _maxSpeed);

                newPosition.z += _accelerationZ * Time.deltaTime;
            }
            // Vérification du bord du bas (mouvement vers l'arrière sur l'axe Z)
            else if (Input.mousePosition.y <= _border)
            {
                _accelerationZ += _accelerationRate * Time.deltaTime;
                _accelerationZ = Mathf.Min(_accelerationZ, _maxSpeed);

                newPosition.z -= _accelerationZ * Time.deltaTime;
            }
            else
            {
                _accelerationZ = 0f;
            }

            // Clamper les limites de la caméra pour éviter de sortir de la zone définie
            newPosition.x = Mathf.Clamp(newPosition.x, MinX, MaxX);
            newPosition.z = Mathf.Clamp(newPosition.z, MinZ, MaxZ);

            _cam.transform.position = newPosition;
        }
    }
}
