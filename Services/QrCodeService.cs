using Net.Codecrete.QrCodeGenerator;
using System.Text;

namespace Nid.Services;

public class QrCodeService
{
    public void PrintQrToConsole(string url)
    {
        // 1. Generate the QR Data
        var qr = QrCode.EncodeText(url, QrCode.Ecc.Medium);

        // 2. Print with a "Quiet Zone" (border) so cameras can see it
        Console.WriteLine("\nScan this to open the upload page:");
        
        for (int y = -2; y < qr.Size + 2; y++)
        {
            for (int x = -2; x < qr.Size + 2; x++)
            {
                // Use the "Full Block" (█) and "Space" characters
                // Note: We use two characters per module to make it look "square"
                bool isBlack = qr.GetModule(x, y);
                Console.Write(isBlack ? "██" : "  ");
            }
            Console.WriteLine();
        }
        Console.WriteLine();
    }
}