using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using UsbNetDllTest;

namespace GanglionUnity.Experimental
{
    public class GanglionUSBController : MonoBehaviour
    {

        [SerializeField] public Text TestText;

        [DllImport("DllTest")] private static extern int Test(int x);

        public void Test()
        {
            TestText.text = "Testing dll: 4+1=" + Test(4) + '\n';

            var dongle = BLE.GetDongle();
            if (dongle == null)
            {
                TestText.text += "Dongle not found.\n";
            }
            else
            {
                TestText.text += "Dongle was found.\n Name:" + dongle.Descriptor.Product;
            }
        }
    }
}