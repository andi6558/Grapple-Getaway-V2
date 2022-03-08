using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


namespace Platformer.Mechanics
{
    public class RopeSystem : MonoBehaviour
    {

        public float climbSpeed = 3f;
        private bool isColliding;

        public GameObject ropeHingeAnchor;
        public DistanceJoint2D ropeJoint;
        public Transform crosshair;
        public SpriteRenderer crosshairSprite;
        public PlayerMovement playerMovement;
        private bool ropeAttached;
        private Vector2 playerPosition;
        private Rigidbody2D ropeHingeAnchorRb;
        private SpriteRenderer ropeHingeAnchorSprite;


        public LineRenderer ropeRenderer;
        public LayerMask ropeLayerMask;
        private float ropeMaxCastDistance = 20f;
        private List<Vector2> ropePositions = new List<Vector2>();


        private bool distanceSet;


        void Awake()
        {

            ropeJoint.enabled = false;
            playerPosition = transform.position;
            ropeHingeAnchorRb = ropeHingeAnchor.GetComponent<Rigidbody2D>();
            ropeHingeAnchorSprite = ropeHingeAnchor.GetComponent<SpriteRenderer>();
        }

        void Update()
        {
            
            var worldMousePosition =
                Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f));
            var facingDirection = worldMousePosition - transform.position;
            var aimAngle = Mathf.Atan2(facingDirection.y, facingDirection.x);
            if (aimAngle < 0f)
            {
                aimAngle = Mathf.PI * 2 + aimAngle;
            }


            var aimDirection = Quaternion.Euler(0, 0, aimAngle * Mathf.Rad2Deg) * Vector2.right;

            playerPosition = transform.position;


            if (!ropeAttached)
            {
                playerMovement.isSwinging = false;
                SetCrosshairPosition(aimAngle);

            }
            else
            {
                playerMovement.isSwinging = true;
                playerMovement.ropeHook = ropePositions.Last();
                crosshairSprite.enabled = false;
            }

            HandleInput(aimDirection);
            UpdateRopePositions();
            HandleRopeLength();

        }

        private void SetCrosshairPosition(float aimAngle)
        {
            if (!crosshairSprite.enabled)
            {
                crosshairSprite.enabled = true;
            }

            var x = transform.position.x + 1f * Mathf.Cos(aimAngle);
            var y = transform.position.y + 1f * Mathf.Sin(aimAngle);

            var crossHairPosition = new Vector3(x, y, 0);
            crosshair.transform.position = crossHairPosition;
        }

        
        private void HandleInput(Vector2 aimDirection)
        {
            if (Input.GetMouseButton(0))
            {
                
                if (ropeAttached) return;
                ropeRenderer.enabled = true;

                var hit = Physics2D.Raycast(playerPosition, aimDirection, ropeMaxCastDistance, ropeLayerMask);

                Debug.Log(hit.collider.name);
                if (hit.collider != null)
                {
                    ropeAttached = true;
                    if (!ropePositions.Contains(hit.point))
                    {
                        
                        // Jump slightly to distance the player a little from the ground after grappling to something.
                        var ropeDirection = (hit.point - playerPosition).normalized;
                        //new Vector2(0f, 2f) + 
                        transform.GetComponent<Rigidbody2D>().AddForce(ropeDirection, ForceMode2D.Impulse);
                        ropePositions.Add(hit.point);
                        ropeJoint.distance = Vector2.Distance(playerPosition, hit.point);
                        ropeJoint.enabled = true;
                        ropeHingeAnchorSprite.enabled = true;
                    }
                }
                
                else
                {
                    ropeRenderer.enabled = false;
                    ropeAttached = false;
                    ropeJoint.enabled = false;
                }
            }

            if (Input.GetAxis("Jump") != 0)
            {
                ResetRope();
            }
        }

        
        private void ResetRope()
        {
            ropeJoint.enabled = false;
            ropeAttached = false;
            playerMovement.isSwinging = false;
            ropeRenderer.positionCount = 2;
            ropeRenderer.SetPosition(0, transform.position);
            ropeRenderer.SetPosition(1, transform.position);
            ropePositions.Clear();
            ropeHingeAnchorSprite.enabled = false;
        }


        private void UpdateRopePositions()
        {
            // 1
            if (!ropeAttached)
            {
                return;
            }

            // 2
            ropeRenderer.positionCount = ropePositions.Count + 1;

            // 3
            for (var i = ropeRenderer.positionCount - 1; i >= 0; i--)
            {
                if (i != ropeRenderer.positionCount - 1) // if not the Last point of line renderer
                {
                    ropeRenderer.SetPosition(i, ropePositions[i]);

                    // 4
                    if (i == ropePositions.Count - 1 || ropePositions.Count == 1)
                    {
                        var ropePosition = ropePositions[ropePositions.Count - 1];
                        if (ropePositions.Count == 1)
                        {
                            ropeHingeAnchorRb.transform.position = ropePosition;
                            if (!distanceSet)
                            {
                                ropeJoint.distance = Vector2.Distance(transform.position, ropePosition);
                                distanceSet = true;
                            }
                        }
                        else
                        {
                            ropeHingeAnchorRb.transform.position = ropePosition;
                            if (!distanceSet)
                            {
                                ropeJoint.distance = Vector2.Distance(transform.position, ropePosition);
                                distanceSet = true;
                            }
                        }
                    }
                    // 5
                    else if (i - 1 == ropePositions.IndexOf(ropePositions.Last()))
                    {
                        var ropePosition = ropePositions.Last();
                        ropeHingeAnchorRb.transform.position = ropePosition;
                        if (!distanceSet)
                        {
                            ropeJoint.distance = Vector2.Distance(transform.position, ropePosition);
                            distanceSet = true;
                        }
                    }
                }
                else
                {
                    // 6
                    ropeRenderer.SetPosition(i, transform.position);
                }
            }
        }

        private void HandleRopeLength()
        {
            // 1
            if (Input.GetAxis("Vertical") >= 1f && ropeAttached)
            {
                ropeJoint.distance -= Time.deltaTime * climbSpeed;
            }
            else if (Input.GetAxis("Vertical") < 0f && ropeAttached)
            {
                ropeJoint.distance += Time.deltaTime * climbSpeed;
            }
        }

        void OnTriggerStay2D(Collider2D colliderStay)
        {
            isColliding = true;
        }

        private void OnTriggerExit2D(Collider2D colliderOnExit)
        {
            isColliding = false;
        }


    }
}
