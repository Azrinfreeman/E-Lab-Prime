# Lab3, Lab4 and Lab5 textbook content

Updated the physical textbooks and relevant runtime writers.

- Lab3: aim, heating observations, boiling-point comparison and expected conclusion. The right page distinguishes simulation reference values (100 C / 104 C) from unrecorded readings. Existing validated user readings still replace the reference page after submission.
- Lab4: temperature, wind, surface area and humidity; expected evaporation trends; guidance to compare rate and time while controlling other factors.
- Lab5: temperature, stirring and particle size; expected dissolving-rate trends. Explains the experiment in terms of rate rather than confusing it with the total amount that can dissolve.

Lab4 and Lab5 retain useful content after recording instead of replacing it with a one-line acknowledgement. Expected trends are labelled as expected; no measured values are invented. Setup tools preserve the new text. Shared Lab3 notebook behaviour outside Lab3 keeps its original fallback text.

Verification: Unity compiled without reported console errors. All six saved page texts checked. Isolated book renders reviewed; the Lab4/5 right pages needed centring, corrected and saved. Runtime writer paths were reviewed, but full submission/quiz/progress flows were not executed. Existing Lab3 validated-result logic is unchanged. Other scene files were not edited.

Evidence: output/labs345-textbooks/ contains backups, initial book previews and final content-checks.json. Initial previews precede the Lab4/5 right-page alignment correction.
