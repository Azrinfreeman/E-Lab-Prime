from html import escape
from pathlib import Path

from reportlab.lib import colors
from reportlab.lib.enums import TA_LEFT, TA_RIGHT
from reportlab.lib.pagesizes import letter
from reportlab.lib.styles import ParagraphStyle, getSampleStyleSheet
from reportlab.lib.units import inch
from reportlab.platypus import KeepTogether, PageBreak, Paragraph, SimpleDocTemplate, Spacer

from build_simplified_documents import GAMIFICATION, LEARNING_VALUE


ROOT = Path(__file__).resolve().parents[2]
PDF_OUT = ROOT / "output" / "pdf"
PDF_OUT.mkdir(parents=True, exist_ok=True)

NAVY = colors.HexColor("#0B2545")
BLUE = colors.HexColor("#2E74B5")
TEAL = colors.HexColor("#11807C")
INK = colors.HexColor("#232C36")
MUTED = colors.HexColor("#5C6773")
RULE = colors.HexColor("#D9E2EA")


def draw_page(canvas, document):
    canvas.saveState()
    width, height = letter
    canvas.setFont("Helvetica-Bold", 8)
    canvas.setFillColor(MUTED)
    canvas.drawString(0.78 * inch, height - 0.47 * inch, "E-LAB PRIME  |  PRESENTATION Q&A")
    canvas.setStrokeColor(RULE)
    canvas.setLineWidth(0.6)
    canvas.line(0.78 * inch, height - 0.55 * inch, width - 0.78 * inch, height - 0.55 * inch)
    canvas.setFont("Helvetica", 8)
    page_text = f"Page {document.page}"
    canvas.drawRightString(width - 0.78 * inch, 0.43 * inch, page_text)
    canvas.restoreState()


def build_styles():
    styles = getSampleStyleSheet()
    return {
        "title": ParagraphStyle(
            "TitleCustom",
            parent=styles["Title"],
            fontName="Helvetica-Bold",
            fontSize=22,
            leading=26,
            textColor=NAVY,
            alignment=TA_LEFT,
            spaceAfter=4,
        ),
        "subtitle": ParagraphStyle(
            "SubtitleCustom",
            parent=styles["Normal"],
            fontName="Helvetica-Oblique",
            fontSize=9.5,
            leading=12,
            textColor=MUTED,
            spaceAfter=13,
        ),
        "section": ParagraphStyle(
            "SectionCustom",
            parent=styles["Heading1"],
            fontName="Helvetica-Bold",
            fontSize=14,
            leading=17,
            textColor=BLUE,
            spaceBefore=12,
            spaceAfter=7,
            keepWithNext=True,
        ),
        "question": ParagraphStyle(
            "QuestionCustom",
            parent=styles["Normal"],
            fontName="Helvetica-Bold",
            fontSize=10.5,
            leading=13.5,
            textColor=NAVY,
            spaceAfter=4,
        ),
        "answer": ParagraphStyle(
            "AnswerCustom",
            parent=styles["Normal"],
            fontName="Helvetica",
            fontSize=10.2,
            leading=14,
            textColor=INK,
            leftIndent=18,
            rightIndent=4,
            spaceAfter=10,
        ),
    }


def build_pdf(title, filename, sections, page_break_before=None):
    output = PDF_OUT / filename
    styles = build_styles()
    story = [
        Paragraph(escape(title), styles["title"]),
        Paragraph("Simplified presentation answers", styles["subtitle"]),
    ]

    number = 1
    for section_title, items in sections:
        story.append(Paragraph(escape(section_title), styles["section"]))
        for question, answer in items:
            if number == page_break_before:
                story.append(PageBreak())
            question_html = f"{number}. {escape(question)}"
            answer_html = f'<font color="#11807C"><b>Answer:</b></font> {escape(answer)}'
            story.append(
                KeepTogether(
                    [
                        Paragraph(question_html, styles["question"]),
                        Paragraph(answer_html, styles["answer"]),
                    ]
                )
            )
            number += 1

    document = SimpleDocTemplate(
        str(output),
        pagesize=letter,
        rightMargin=0.78 * inch,
        leftMargin=0.78 * inch,
        topMargin=0.72 * inch,
        bottomMargin=0.65 * inch,
        title=title,
        author="E-Lab Prime Project Team",
        subject="Simplified presentation questions and answers",
    )
    document.build(story, onFirstPage=draw_page, onLaterPages=draw_page)
    print(output)


def main():
    build_pdf(
        "Pedagogy & Learning Value",
        "Pedagogy and Learning Value - Simplified.pdf",
        LEARNING_VALUE,
        page_break_before=6,
    )
    build_pdf(
        "Pedagogy vs. Gamification",
        "Pedagogy vs Gamification - Simplified.pdf",
        GAMIFICATION,
    )


if __name__ == "__main__":
    main()



