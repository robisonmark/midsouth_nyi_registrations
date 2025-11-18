from typing import Optional

from enums import ShirtSize
from reportlab.lib import colors
from reportlab.lib.pagesizes import LETTER
from reportlab.lib.styles import ParagraphStyle
from reportlab.lib.units import inch
from reportlab.pdfgen import canvas
from reportlab.platypus import (
    PageBreak,
    Paragraph,
    SimpleDocTemplate,
    Spacer,
    Table,
    TableStyle,
)


class SwagPDFGenerator:
    """Generates PDF reports for swag (shirt) orders by church."""

    # Standard shirt sizes in order
    SHIRT_SIZES = [shirt_size.value for shirt_size in ShirtSize.__members__.values()]

    def __init__(self, font_name: str = "Helvetica", font_size: int = 8):
        self.font_name = font_name
        self.font_size = font_size
        self.page_width, self.page_height = LETTER

    def _create_title(self, church: str, age_group: Optional[str] = None, is_roster: bool = False) -> Paragraph:
        """Create a title paragraph for a section."""
        title_text = church
        if age_group:
            title_text += f" - {age_group}"
        if is_roster:
            title_text += " Roster"

        title_style = ParagraphStyle(
            "CustomTitle", fontName=f"{self.font_name}-Bold", fontSize=self.font_size + 4, spaceAfter=16
        )
        return Paragraph(title_text, title_style)

    def _create_subtitle(self, text: str) -> Paragraph:
        """Create a subtitle paragraph."""
        subtitle_style = ParagraphStyle(
            "SubTitle", fontName=f"{self.font_name}-Bold", fontSize=self.font_size + 2, spaceAfter=12, spaceBefore=16
        )
        return Paragraph(text, subtitle_style)

    def _create_totals_table(self, totals: dict, title: str = None) -> list:
        """Create a totals table with checkbox grid format. Returns list of flowables."""
        elements = []

        # Add subtitle if provided
        if title:
            elements.append(self._create_subtitle(title))

        # Prepare data for the table
        data = [[" ", "Size", "Count"]]

        # Add each shirt size
        for size in self.SHIRT_SIZES:
            count = totals.get(size, 0)
            data.append([" ", size, str(count)])

        # Add total row
        total_count = sum(totals.values())
        data.append(["", "TOTAL", str(total_count)])

        # Create table
        table = Table(data, colWidths=[0.5 * inch, 1.5 * inch, 1.5 * inch])

        # Style the table
        table.setStyle(
            TableStyle(
                [
                    # Header row
                    ("BACKGROUND", (0, 0), (-1, 0), colors.grey),
                    ("TEXTCOLOR", (0, 0), (-1, 0), colors.whitesmoke),
                    ("ALIGN", (0, 0), (-1, -1), "CENTER"),
                    ("FONTNAME", (0, 0), (-1, 0), f"{self.font_name}-Bold"),
                    ("FONTSIZE", (0, 0), (-1, -1), self.font_size),
                    ("BOTTOMPADDING", (0, 0), (-1, 0), 12),
                    ("TOPPADDING", (0, 0), (-1, -1), 8),
                    ("BOTTOMPADDING", (0, 1), (-1, -1), 8),
                    # Grid
                    ("GRID", (0, 0), (-1, -2), 1, colors.black),
                    ("BOX", (0, 0), (-1, -2), 2, colors.black),
                    # Total row
                    ("LINEABOVE", (0, -1), (-1, -1), 2, colors.black),
                    ("FONTNAME", (1, -1), (-1, -1), f"{self.font_name}-Bold"),
                    ("BACKGROUND", (0, -1), (-1, -1), colors.lightgrey),
                ]
            )
        )

        elements.append(table)
        return elements

    def _create_combined_totals_page(
        self, church: str, totals: dict, late_totals: dict, age_group: Optional[str] = None
    ) -> list:
        """Create a combined page with regular totals and late totals."""
        elements = []

        # Add main title
        elements.append(self._create_title(church, age_group))

        # Add regular totals
        elements.extend(self._create_totals_table(totals))

        # Add late totals if they exist
        if late_totals:
            elements.extend(self._create_totals_table(late_totals, "Late Registrations (After Cutoff)"))

        return elements

    def _create_roster_table(self, roster: list) -> Table:
        """Create a roster table with names and shirt sizes."""
        # Sort roster by name
        sorted_roster = sorted(roster, key=lambda x: x["name"])

        # Prepare data for the table
        data = [["#", "Name", "Shirt Size"]]

        for idx, person in enumerate(sorted_roster, 1):
            data.append([str(idx), person["name"], person["shirt_size"]])

        # Add total row
        data.append(["", f"Total: {len(sorted_roster)}", ""])

        # Create table with appropriate column widths
        table = Table(data, colWidths=[0.5 * inch, 4.5 * inch, 1.5 * inch])

        # Style the table
        table.setStyle(
            TableStyle(
                [
                    # Header row
                    ("BACKGROUND", (0, 0), (-1, 0), colors.grey),
                    ("TEXTCOLOR", (0, 0), (-1, 0), colors.whitesmoke),
                    ("ALIGN", (0, 0), (0, -1), "CENTER"),  # Number column centered
                    ("ALIGN", (1, 0), (1, -1), "LEFT"),  # Name column left-aligned
                    ("ALIGN", (2, 0), (2, -1), "CENTER"),  # Size column centered
                    ("FONTNAME", (0, 0), (-1, 0), f"{self.font_name}-Bold"),
                    ("FONTSIZE", (0, 0), (-1, -1), self.font_size),
                    ("BOTTOMPADDING", (0, 0), (-1, 0), 12),
                    ("TOPPADDING", (0, 0), (-1, -1), 6),
                    ("BOTTOMPADDING", (0, 1), (-1, -1), 6),
                    # Horizontal lines
                    ("LINEBELOW", (0, 0), (-1, 0), 2, colors.black),  # Below header
                    ("LINEBELOW", (0, 1), (-1, -2), 0.5, colors.grey),  # Between rows
                    # Total row
                    ("LINEABOVE", (0, -1), (-1, -1), 2, colors.black),
                    ("FONTNAME", (0, -1), (-1, -1), f"{self.font_name}-Bold"),
                    ("BACKGROUND", (0, -1), (-1, -1), colors.lightgrey),
                ]
            )
        )

        return table

    def generate_combined_pdf(self, swag_manager, filename: str) -> None:
        """
        Create a single PDF with all churches' totals followed by all rosters.

        Args:
            swag_manager: SwagManager instance
            filename: Output PDF filename (full path)
        """
        doc = SimpleDocTemplate(filename, pagesize=LETTER)
        elements = []

        age_groups = swag_manager.get_all_age_groups()

        # Section 1: All Church Totals (with late totals on same page)
        for age_group in age_groups:
            churches = swag_manager.get_all_churches(age_group)

            for church in churches:
                totals = swag_manager.get_church_totals(church, age_group)
                late_totals = swag_manager.get_church_totals(church, age_group, include_late=True)

                if not totals and not late_totals:
                    continue

                # Add combined totals page (regular + late)
                elements.extend(self._create_combined_totals_page(church, totals, late_totals, age_group))

                # Add page break after each church
                elements.append(PageBreak())

        # Section 2: All Church Rosters
        for age_group in age_groups:
            churches = swag_manager.get_all_churches(age_group)

            for church in churches:
                roster = swag_manager.get_church_roster(church, age_group)

                if not roster:
                    continue

                # Add title
                elements.append(self._create_title(church, age_group, is_roster=True))

                # Add roster table
                elements.append(self._create_roster_table(roster))

                # Add page break after each church
                elements.append(PageBreak())

        # Build PDF
        doc.build(elements)

    def generate_all_pdfs(self, swag_manager, output_dir: str) -> None:
        """
        Create a single combined PDF with all swag information.

        Args:
            swag_manager: SwagManager instance
            output_dir: Directory to save PDF (will be created if needed)
        """
        from datetime import datetime
        from pathlib import Path

        # Ensure output directory exists
        Path(output_dir).mkdir(parents=True, exist_ok=True)

        # Create single combined PDF filename with timestamp
        timestamp = datetime.now().strftime("%Y_%m_%d")
        event_type = swag_manager.event_type.title()
        filename = f"{output_dir}/{timestamp}_{event_type}_Swag_Report.pdf"

        self.generate_combined_pdf(swag_manager, filename)

        print(f"✅ Created combined swag report: {filename}")

        # Print summary
        age_groups = swag_manager.get_all_age_groups()
        total_churches = 0
        total_shirts = 0
        late_shirts = 0

        for age_group in age_groups:
            churches = swag_manager.get_all_churches(age_group)
            total_churches += len(churches)
            for church in churches:
                totals = swag_manager.get_church_totals(church, age_group)
                late_totals = swag_manager.get_church_totals(church, age_group, include_late=True)
                total_shirts += sum(totals.values())
                late_shirts += sum(late_totals.values())

        print(f"   📊 {total_churches} churches")
        print(f"   👕 {total_shirts} guaranteed shirts")

        if swag_manager.cutoff_date and late_shirts > 0:
            print(f"   ⏰ {late_shirts} late registrations (after {swag_manager.cutoff_date.strftime('%b %d, %Y')})")
            print(f"   📦 {total_shirts + late_shirts} total shirts")
