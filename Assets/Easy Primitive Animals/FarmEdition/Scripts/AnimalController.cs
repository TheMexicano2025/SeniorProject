using System.Collections;
using UnityEngine;

namespace EasyPrimitiveAnimals
{
    public class AnimalController : MonoBehaviour
    {
        public GameObject FrontLegL;
        public GameObject FrontLegR;
        public GameObject RearLegL;
        public GameObject RearLegR;

        private Vector3 legStartPosA = new Vector3(10.0f, 0f, 0f);
        private Vector3 legEndPosA = new Vector3(-10.0f, 0f, 0f);

        private Vector3 legStartPosB = new Vector3(-10.0f, 0f, 0f);
        private Vector3 legEndPosB = new Vector3(10.0f, 0f, 0f);

        private float rotSpeed;

        public float moveAngle = 90f;
        public float movSpeed = 1f;

        private bool canRotate = true;
        private bool canPeck = true;

        private void Start()
        {
            if (!this.gameObject.CompareTag("Chicken"))
            {
                FrontLegL = transform.Find("BaseAnimal").transform.Find("Legs").transform.Find("EPA_FL").gameObject;
                FrontLegR = transform.Find("BaseAnimal").transform.Find("Legs").transform.Find("EPA_FR").gameObject;
                RearLegL = transform.Find("BaseAnimal").transform.Find("Legs").transform.Find("EPA_RL").gameObject;
                RearLegR = transform.Find("BaseAnimal").transform.Find("Legs").transform.Find("EPA_RR").gameObject;

                rotSpeed = movSpeed * 4;
            }
        }

        private void Update()
        {
            if (!this.gameObject.CompareTag("Chicken"))
            {
                Quaternion legAngleFromA = Quaternion.Euler(this.legStartPosA);
                Quaternion legAngleToA = Quaternion.Euler(this.legEndPosA);

                Quaternion legAngleFromB = Quaternion.Euler(this.legStartPosB);
                Quaternion legAngleToB = Quaternion.Euler(this.legEndPosB);

                float lerp = 0.5f * (1.0f + Mathf.Sin(Mathf.PI * Time.realtimeSinceStartup * this.rotSpeed));

                FrontLegL.transform.localRotation = Quaternion.Lerp(legAngleFromA, legAngleToA, lerp);
                FrontLegR.transform.localRotation = Quaternion.Lerp(legAngleFromB, legAngleToB, lerp);

                RearLegL.transform.localRotation = Quaternion.Lerp(legAngleFromB, legAngleToB, lerp);
                RearLegR.transform.localRotation = Quaternion.Lerp(legAngleFromA, legAngleToA, lerp);

                transform.Translate((Vector3.forward * Time.deltaTime) * movSpeed);
            } 
            else
            {
                if (Random.Range(0, 100) > 50 && canPeck)
                {
                    StartCoroutine(TimeToPeck());
                }
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!collision.gameObject.CompareTag("Ground") && canRotate)
            {
                StartCoroutine(SpinMeRound());
            }
        }

        private IEnumerator SpinMeRound()
        {
            canRotate = false;
            this.transform.rotation *= Quaternion.Euler(0, moveAngle, 0);
            yield return new WaitForSeconds(1f);
            canRotate = true;
        }

        private IEnumerator TimeToPeck()
        {
            canPeck = false;
            this.transform.eulerAngles = new Vector3(45f, transform.eulerAngles.y, transform.eulerAngles.z);
            yield return new WaitForSeconds(0.2f);
            this.transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, transform.eulerAngles.z);
            yield return new WaitForSeconds(Random.Range(3f, 7f));
            canPeck = true;
        }
    }
}
