using Net.Codecrete.QrCodeGenerator;

namespace Nid.Services;

public class QrCodeService
{
    public void PrintQrToConsole(string url)
    {
        // 1. Generate the QR Data
        var qr = QrCode.EncodeText(url, QrCode.Ecc.Medium);

        // 2. Print with a "Quiet Zone" (border) so cameras can see it
        Console.WriteLine("\nScan this to open the upload page:");
        
        // Doubling the characters because terminal characters are about twice as tall as they're wide.
        // So to make a square we need two of them.
        for (int y = -2; y < qr.Size + 2; y++)
        {
            for (int x = -2; x < qr.Size + 2; x++)
            {
                // Use the "Full Block" (█) and "Space" characters
                bool isBlack = qr.GetModule(x, y);
                Console.Write(isBlack ? "██" : "  ");
            }
            Console.WriteLine();
        }
        Console.WriteLine();
    }
}
