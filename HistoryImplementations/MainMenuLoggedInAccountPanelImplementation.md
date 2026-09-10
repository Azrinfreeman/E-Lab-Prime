# Main Menu Logged-In Account Panel Implementation

## Goal

Improve the main menu account panel so a logged-in player sees their current student profile instead of the login/register form.

When the player is signed in, the account panel should clearly communicate:

- The player is currently logged in.
- Which student profile is active.
- Whether the session is a real student profile or guest mode.
- Progress will be saved for this profile.
- The player can log out from this panel.

This makes the account area feel more professional and avoids the confusing state shown in the current screenshot, where the account panel still displays `Log Masuk` and `Daftar` even though the side badge says `ACTIVE`.

## Current State

Existing controller:

```text
Assets/Script/MainMenuAccountPanelController.cs
```

Existing session badge:

```text
Assets/Script/MainMenuSessionStatusUI.cs
```

Existing account setup helper:

```text
Assets/Script/Editor/MainMenuAccountPanelSetup.cs
```

Current account state is stored in `PlayerPrefs`:

```text
ELabPrime_AccountSignedIn
ELabPrime_AccountGuest
ELabPrime_AccountName
ELabPrime_AccountStudentId
ELabPrime_AccountClass
ELabPrime_AccountPassword
```

The account controller already has fields for:

```csharp
public GameObject loginForm;
public GameObject registerForm;
public GameObject signedInPanel;
public TextMeshProUGUI signedInNameText;
public TextMeshProUGUI signedInDetailsText;
public Button logoutButton;
```

So the system is already partially prepared for this feature.

## Current Problem

`MainMenuSessionStatusUI.OpenAccountPanelImmediate()` always calls:

```csharp
accountController.ShowLoginTab();
```

That forces:

- login form visible
- register form hidden
- signed-in panel hidden

This happens even if:

```text
ELabPrime_AccountSignedIn = 1
```

Result:

- Side badge says `ACTIVE`.
- Account panel still asks the player to log in.
- The professor guide may show sign-in guidance even though the session is active.

## Target Behavior

### Signed Out

Show the current login/register UI:

- Login tab.
- Register tab.
- Student ID input.
- Password input.
- Continue as guest.
- Status message.

### Signed In As Student

Hide login/register forms and show a profile overview:

```text
AKAUN PELAJAR
(STUDENT ACCOUNT)

[ACTIVE PROFILE]

Name:  Aiman Hakim
ID:    STU001
Class: 1 Bestari
Mode:  Student profile

Progress saving: Active
Last session: This device

[CONTINUE LEARNING] [LOG OUT]
```

### Signed In As Guest

Hide login/register forms and show a guest overview:

```text
MOD TETAMU AKTIF
(GUEST MODE)

Name: Tetamu
Mode: Guest session
Progress: Saved only on this device

[REGISTER STUDENT PROFILE] [LOG OUT]
```

## Recommended UI Upgrade

Replace the signed-in area with a polished "Student Identity Card" style panel.

Suggested visual pieces:

- Circular avatar badge with initials.
- Green `ACTIVE` pill for student login.
- Yellow `GUEST` pill for guest mode.
- Profile detail rows with small icons/labels.
- Progress-save confirmation strip.
- Logout button with warning color.
- Continue button to return to main menu or close account panel.

Suggested hierarchy inside `PanelLogin`:

```text
SignedInPanel
  ProfileHeader
    AvatarCircle
      InitialsText
    ProfileNameText
    ProfileModePill
  DetailGrid
    StudentIdRow
    ClassRow
    SaveStatusRow
    DeviceStatusRow
  ProgressSummary
    ProgressTitleText
    ProgressSubtitleText
  ActionRow
    ContinueButton
    LogoutButton
```

## Polish Ideas

### Profile Initials Avatar

Generate initials from the student name:

```text
Nur Aina Sofia -> NAS or NA
Pelajar E-Lab -> PE
Tetamu -> T
```

If no name exists:

```text
EL
```

### Session Status Copy

Student:

```text
Profil aktif. Kemajuan amali akan disimpan.
```

Guest:

```text
Mod tetamu aktif. Kemajuan hanya disimpan pada peranti ini.
```

Signed out:

```text
Log masuk untuk simpan progress pelajar.
```

### Extra Premium Details

Add small data points:

- `Access Level: Student`
- `Save Sync: Local Device`
- `Current Build: v0.1.2`
- `Lab Access: Ready`

These are low-risk because they do not require a backend.

## Required Code Changes

## 1. Add Account Overview Method

Add a public method to `MainMenuAccountPanelController`:

```csharp
public void ShowAccountOverview()
{
    RefreshState();

    if (!IsSignedIn())
    {
        ShowLoginTab();
        return;
    }

    if (loginForm != null)
    {
        loginForm.SetActive(false);
    }

    if (registerForm != null)
    {
        registerForm.SetActive(false);
    }

    if (signedInPanel != null)
    {
        signedInPanel.SetActive(true);
    }
}
```

This becomes the safe entry point whenever the account panel is opened from the session strip or side badge.

## 2. Update Session Status Panel Routing

Change `MainMenuSessionStatusUI.OpenAccountPanelImmediate()` from:

```csharp
accountController.ShowLoginTab();
```

To:

```csharp
accountController.ShowAccountOverview();
```

This fixes the active-session bug.

## 3. Improve RefreshState

`RefreshState()` already handles most state toggling. It should be adjusted so:

- Signed-in student shows `signedInPanel`.
- Guest shows `signedInPanel` with guest-specific copy.
- Signed-out shows login tab by default.
- Register form is not activated unless the player selects the register tab.

## 4. Add More Signed-In Text Fields

Current fields:

```csharp
signedInNameText
signedInDetailsText
```

Recommended additions:

```csharp
public TextMeshProUGUI signedInInitialsText;
public TextMeshProUGUI signedInModeText;
public TextMeshProUGUI signedInSaveStatusText;
public TextMeshProUGUI signedInAccessText;
public Image signedInModePillImage;
```

Optional:

```csharp
public Button continueButton;
```

## 5. Add Helper Methods

Add:

```csharp
private string GetInitials()
private string GetModeLabel()
private string GetSaveStatus()
private string GetAccessLabel()
```

Example output:

Student:

```text
Mode: PROFIL PELAJAR
Save: Progress save active
Access: Lab access ready
```

Guest:

```text
Mode: MOD TETAMU
Save: Local device only
Access: Limited profile
```

## 6. Account Panel Setup

Expand `MainMenuAccountPanelSetup.cs` from reference validation into a setup/repair utility:

```text
E-Lab Prime/Setup Main Menu Account Panel
```

Responsibilities:

- Find `PanelLogin`.
- Find or create `SignedInPanel`.
- Create the profile-card UI if missing.
- Wire `signedInNameText`.
- Wire `signedInDetailsText`.
- Wire logout button.
- Wire new polish fields.
- Mark scene dirty.

Keep the existing validation menu item too.

## Suggested Layout

For the current screenshot, the signed-in panel should occupy the same area as the login/register form.

Approximate layout:

```text
Top:
  AKAUN PELAJAR
  (STUDENT ACCOUNT)

Middle:
  [Avatar]  NAMA PELAJAR
            ID: 12345 | Kelas: 1 Bestari
            [ACTIVE PROFILE]

Info row:
  [SAVE ACTIVE] [LAB ACCESS READY] [LOCAL DEVICE]

Bottom:
  [KEMBALI KE MENU] [LOG KELUAR]
```

The login/register tabs should be hidden while signed in because they are no longer relevant.

## Guest Mode UX

Guest mode should not look like an error. It should look intentional but slightly limited.

Guest panel copy:

```text
MOD TETAMU AKTIF
Progress hanya disimpan pada peranti ini.
Daftar profil pelajar untuk simpan nama, kelas dan ID pelajar.
```

Buttons:

```text
[DAFTAR PROFIL PELAJAR] [LOG KELUAR]
```

Implementation:

- `Register Student Profile` calls `ShowRegisterTab()`.
- `Logout` calls `Logout()`.

## Professor Guide Integration

When signed in:

- Professor should use `SignedInWelcome`.
- Account hover should not say "Log masuk dahulu".
- If account panel is opened while signed in, professor copy can later become:

```text
Profil pelajar anda aktif. Kemajuan amali akan disimpan secara automatik.
```

This can be added as a later dialogue context if we want more polish.

## Validation Checklist

### Signed Out

- Open account panel.
- Login/register tabs visible.
- Signed-in profile card hidden.
- Status says login is needed.

### Register

- Register new player.
- Panel immediately switches to profile overview.
- Side badge says `ACTIVE`.
- Session strip says `AKSES PELAJAR AKTIF`.
- Login/register tabs are hidden.

### Login

- Log out.
- Log in using saved ID/password.
- Panel switches to profile overview.
- Name, ID and class are displayed.

### Guest

- Continue as guest.
- Panel shows guest profile overview.
- Badge says `GUEST`.
- Guest message explains local-only progress.

### Logout

- Click logout.
- Signed-in panel hides.
- Login form returns.
- Side badge returns to `SIGN IN`.
- Professor guide can show sign-in prompt again in a new play session.

## Risk Notes

- This is a local-only account system using `PlayerPrefs`, not a secure online login.
- Password is currently stored in `PlayerPrefs`, which is fine for prototype/local classroom use but not suitable for real online authentication.
- Avoid presenting this as secure cloud account login until a real backend exists.

## Recommended Phase 1 Scope

Implement only:

- Correct signed-in routing.
- Student profile card.
- Guest profile card.
- Logout button.
- Register-from-guest button.
- Updated session badge refresh.
- Unity console validation.
- Screenshot verification.

Leave backend/authentication/security upgrades for a later phase.

