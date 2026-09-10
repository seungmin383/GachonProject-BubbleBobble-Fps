using UnityEngine;

namespace Asset.Script.Monster
{

    public class BasicMonster : MonoBehaviour
    {
        private GameObject _bubble;

        private void Update()
        {
            if( _bubble == null )
            {
                return;
            }

            transform.position = _bubble.transform.position;
        }

        public void Capture(GameObject bubble)
        {
            _bubble = bubble;
        }
        public void Chain()
        {
            _bubble = null;
            Destroy(gameObject);
        }
    }
}