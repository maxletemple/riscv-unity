using UnityEngine;
using TMPro;

public class Console : MonoBehaviour
{
    private TextMeshProUGUI textMeshPro;
    private char[,] consoleGrid;
    public int width = 80;
    public int height = 25;

    private Vector2Int cursorPosition;

    enum CharStatus
    {
        text,
        ESC,
        control
    }

    private CharStatus charStatus = CharStatus.text;

    void Start()
    {
        consoleGrid = new char[height, width];
        cursorPosition = new Vector2Int(0, 0);
        textMeshPro = GetComponent<TextMeshProUGUI>();
        // Initialize the console grid with spaces
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                consoleGrid[y, x] = ' ';
            }
        }
        UpdateDisplay();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        string displayText = "";
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                displayText += consoleGrid[y, x];
            }
            displayText += "\n";
        }
        textMeshPro.text = displayText;
    }

    public void WriteChar(byte c)
    {
        switch (charStatus)
        {
            case CharStatus.text:
                switch (c)
                {
                    case 0x1B: // ESC
                        charStatus = CharStatus.ESC;
                        break;
                    case 0x08: // Backspace
                        if (cursorPosition.x > 0)
                        {
                            cursorPosition.x--;
                            consoleGrid[cursorPosition.y, cursorPosition.x] = ' ';
                        }
                        break;
                    case 0x0A: // Newline
                        cursorPosition.x = 0;
                        if (cursorPosition.y < height - 1)
                        {
                            cursorPosition.y++;
                        }
                        break;
                    case 0x0D: // Carriage return
                        cursorPosition.x = 0;
                        break;
                    default:
                        if (cursorPosition.x < width)
                        {
                            consoleGrid[cursorPosition.y, cursorPosition.x] = (char)c;
                            cursorPosition.x++;
                        }
                        break;
                }
                break;
            case CharStatus.ESC:
                switch (c)
                {
                    case 0x5B: // CSI
                        charStatus = CharStatus.control;
                        break;
                    default:
                        charStatus = CharStatus.text; // Reset to text mode for unrecognized ESC sequences
                        break;
                }
                break;
            case CharStatus.control:
                switch (c)
                {
                    case 0x41: // Up arrow
                        if (cursorPosition.y > 0)
                        {
                            cursorPosition.y--;
                        }
                        break;
                    case 0x42: // Down arrow
                        if (cursorPosition.y < height - 1)
                        {
                            cursorPosition.y++;
                        }
                        break;
                    case 0x43: // Right arrow
                        if (cursorPosition.x < width - 1)
                        {
                            cursorPosition.x++;
                        }
                        break;
                    case 0x44: // Left arrow
                        if (cursorPosition.x > 0)
                        {
                            cursorPosition.x--;
                        }
                        break;
                    default:
                        charStatus = CharStatus.text; // Reset to text mode for unrecognized control sequences
                        break;
                }
                charStatus = CharStatus.text; // Reset to text mode after processing control sequence
                break;
        }
    }
}
