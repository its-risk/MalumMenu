using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace MalumMenu;

[HarmonyPatch(typeof(TextBoxTMP), nameof(TextBoxTMP.Update))]
public static class TextBoxTMP_Update
{
    // Postfix patch of TextBoxTMP.Update to allow copying, pasting and cutting text between the chatbox and the device's clipboard
    public static void Postfix(TextBoxTMP __instance)
    {
        if (!CheatToggles.chatJailbreak || !__instance.hasFocus) return;

        if (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl)) return;

        if (Input.GetKeyDown(KeyCode.C))
        {
            GUIUtility.systemCopyBuffer = __instance.text;
        }
        if (Input.GetKeyDown(KeyCode.V))
        {
            __instance.SetText(__instance.text + GUIUtility.systemCopyBuffer);
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            GUIUtility.systemCopyBuffer = __instance.text;
            __instance.SetText("");
        }
    }
}

[HarmonyPatch(typeof(TextBoxTMP), nameof(TextBoxTMP.IsCharAllowed))]
public static class TextBoxTMP_IsCharAllowed
{
    private static int _currentCharPos = 0;

    // Prefix patch of TextBoxTMP.IsCharAllowed to allow all characters
    public static bool Prefix(TextBoxTMP __instance, ref bool __result)
    {
        // If user is writing through IME composition, then always allow the inputted characters
        // Fixes issues for users of CJK languages

        string compositionString = Input.compositionString;
        if (compositionString.Length > 0)
        {
            __result = true;
            return false;
        }

        string inputString = Input.inputString;

        if (inputString.Length == 0) return true;

        string currentText = __instance.text ?? string.Empty;

        int caretPos = Mathf.Clamp(__instance.caretPos, 0, currentText.Length);

        string text = currentText.Insert(caretPos, inputString);

        char currentChar = text[_currentCharPos];

        if (_currentCharPos == text.Length - 1)
        {
            _currentCharPos = 0;
        }
        else
        {
            _currentCharPos++;
        }

        if (CheatToggles.chatJailbreak)
        {
            HashSet<char> blockedSymbols = new() { '\b', '\r', '>', '<' };

            if (blockedSymbols.Contains(currentChar))
            {
                __result = false;
                return false;
            }

            __result = true;
        }
        else
        {
            if (__instance.IpMode)
            {
                __result = (currentChar >= '0' && currentChar <= '9') || currentChar == '.';
                return false;
            }

            __result = currentChar == ' ' ||
            (currentChar >= 'A' && currentChar <= 'Z') ||
            (currentChar >= 'a' && currentChar <= 'z') ||
            (currentChar >= '0' && currentChar <= '9') ||
            (currentChar >= 'À' && currentChar <= 'ÿ') ||
            (currentChar >= 'Ѐ' && currentChar <= 'џ') ||
            (currentChar >= '぀' && currentChar <= '㆟') ||
            (currentChar >= 'ⱡ' && currentChar <= '힣') ||
            (__instance.AllowSymbols && TextBoxTMP.SymbolChars.Contains(currentChar)) ||
            (__instance.AllowEmail && TextBoxTMP.EmailChars.Contains(currentChar));
        }

        return false;
    }
}
