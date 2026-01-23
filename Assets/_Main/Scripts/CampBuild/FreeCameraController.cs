using UnityEngine;
namespace CampBuild
{
    public class FreeCameraController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f; 
        [SerializeField] private float lookSpeed = 2f; 
        [SerializeField] private float minHeight = 1f; 
        [SerializeField] private float maxHeight = 10f; 

        private bool _isActive = false; 
        private bool _isRotating = false; 
        private Vector3 _initialPosition; 
        private Quaternion _initialRotation; 
        private float _rotationX = 0f; 

        private void Start()
        {
            _initialPosition = transform.position;
            _initialRotation = transform.rotation;
        }

        private void Update()
        {
            if (!_isActive) return; 

            MoveCamera(); 
            RotateCamera(); 


            Vector3 position = transform.position;
            position.y = Mathf.Clamp(position.y, minHeight, maxHeight); 
            transform.position = position;
        }

        private void MoveCamera()
        {
            float horizontal = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
            float vertical = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;

            transform.Translate(horizontal, 0f, vertical);
        }

        private float _rotationY;

        private void RotateCamera()
        {
            if (Input.GetMouseButtonDown(2))
                _isRotating = true;

            if (Input.GetMouseButtonUp(2))
                _isRotating = false;

            if (!_isRotating)
                return;

            float mouseX = Input.GetAxis("Mouse X") * lookSpeed;

            _rotationY += mouseX;

            transform.localRotation = Quaternion.Euler(
                _initialRotation.eulerAngles.x, 
                _rotationY,                     
                0f                            
            );
        }


        public void SetActive(bool isActive)
        {
            _isActive = isActive;

            if (!_isActive)
            {
                transform.position = _initialPosition;
                transform.rotation = _initialRotation;
            }
        }
    }
}