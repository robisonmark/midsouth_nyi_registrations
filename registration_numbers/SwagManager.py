from datetime import datetime
from typing import Any, Optional

from config import AGE_GROUPS
from enums import Camp, RegistrationType


class SwagManager:
    """
    Manages swag (shirts) for different events (Camp, Momentum).
    Tracks totals and rosters by church and optionally by age group.
    """

    def __init__(self, event_type: str, age_group_specific: bool = False, cutoff_date: Optional[datetime] = None):
        """
        Initialize the SwagManager.

        Args:
            event_type: "camp" or "momentum"
            age_group_specific: True for Camp (split by age), False for Momentum (all together)
            cutoff_date: Optional datetime to track late registrations
        """
        self.event_type = event_type.lower()
        self.age_group_specific = age_group_specific
        self.cutoff_date = cutoff_date

        # Structure depends on whether we're splitting by age group
        if age_group_specific:
            self._totals = {AGE_GROUPS.HIGH_SCHOOL: {}, AGE_GROUPS.MIDDLE_SCHOOL: {}}
            self._roster = {AGE_GROUPS.HIGH_SCHOOL: {}, AGE_GROUPS.MIDDLE_SCHOOL: {}}
            self._late_totals = {AGE_GROUPS.HIGH_SCHOOL: {}, AGE_GROUPS.MIDDLE_SCHOOL: {}}
        else:
            self._totals = {}
            self._roster = {}
            self._late_totals = {}

    def _get_age_group_from_camp(self, camp) -> Optional[str]:
        """Convert Camp enum to age group string."""
        if camp == Camp.HIGH_SCHOOL:
            return AGE_GROUPS.HIGH_SCHOOL
        elif camp == Camp.MIDDLE_SCHOOL:
            return AGE_GROUPS.MIDDLE_SCHOOL
        return None

    def _ensure_church_exists(self, church: str, age_group: Optional[str] = None) -> None:
        """Ensure church entry exists in all tracking dictionaries."""
        if self.age_group_specific and age_group:
            # For age-group specific events (Camp)
            if church not in self._totals[age_group]:
                self._totals[age_group][church] = {}
            if church not in self._roster[age_group]:
                self._roster[age_group][church] = []
            if church not in self._late_totals[age_group]:
                self._late_totals[age_group][church] = {}
        else:
            # For non-age-group specific events (Momentum)
            if church not in self._totals:
                self._totals[church] = {}
            if church not in self._roster:
                self._roster[church] = []
            if church not in self._late_totals:
                self._late_totals[church] = {}

    def add_registrant(self, registrant) -> None:
        """
        Add a registrant to the swag tracking system.

        Args:
            registrant: Either a Camper or MomentumRegistrant object
        """
        # Skip if deleted or archived
        if hasattr(registrant, "approval_status") and registrant.approval_status.lower() in ["deleted", "archived"]:
            return

        # Determine church - prefer actual church over Staff
        church = registrant.church
        if not church or church == "":
            # Only default to Staff if no church AND they are actually staff
            if hasattr(registrant, "registration_type") and registrant.registration_type == RegistrationType.STAFF:
                church = "Staff"
            else:
                church = "Unknown"

        shirt_size = registrant.shirt_size.value if hasattr(registrant.shirt_size, "value") else str(registrant.shirt_size)
        name = f"{registrant.first_name} {registrant.last_name}"

        # Determine if this is a late registration
        is_late = False
        if self.cutoff_date:
            reg_date_str = registrant.registration_type_date
            if reg_date_str:
                try:
                    # Handle various date formats
                    if isinstance(reg_date_str, datetime):
                        reg_date = reg_date_str
                    elif "T" in reg_date_str or "Z" in reg_date_str:
                        reg_date = datetime.fromisoformat(reg_date_str.replace("Z", "+00:00"))
                    else:
                        # Try multiple date formats
                        for fmt in ["%Y-%m-%d %H:%M:%S", "%b %d, %Y", "%Y-%m-%d"]:
                            try:
                                reg_date = datetime.strptime(reg_date_str, fmt)
                                break
                            except ValueError:
                                continue
                        else:
                            reg_date = None

                    if reg_date:
                        is_late = reg_date > self.cutoff_date
                except (ValueError, AttributeError):
                    pass

        if self.age_group_specific:
            # For Camp - handle by age group
            if hasattr(registrant, "camp"):
                age_group = self._get_age_group_from_camp(registrant.camp)
                if age_group:
                    self._add_to_tracking(church, name, shirt_size, age_group, is_late)
                else:
                    # Both camps - add to both age groups
                    self._add_to_tracking(church, name, shirt_size, AGE_GROUPS.MIDDLE_SCHOOL, is_late)
                    self._add_to_tracking(church, name, shirt_size, AGE_GROUPS.HIGH_SCHOOL, is_late)
        else:
            # For Momentum - no age group split
            self._add_to_tracking(church, name, shirt_size, None, is_late)

    def _add_to_tracking(self, church: str, name: str, shirt_size: str, age_group: Optional[str], is_late: bool) -> None:
        """Add a person to totals and roster tracking."""
        self._ensure_church_exists(church, age_group)

        if self.age_group_specific and age_group:
            # Update roster (all registrations go in roster)
            self._roster[age_group][church].append({"name": name, "shirt_size": shirt_size, "is_late": is_late})

            # Update totals - late registrations ONLY go in late_totals
            if is_late:
                if shirt_size not in self._late_totals[age_group][church]:
                    self._late_totals[age_group][church][shirt_size] = 0
                self._late_totals[age_group][church][shirt_size] += 1
            else:
                if shirt_size not in self._totals[age_group][church]:
                    self._totals[age_group][church][shirt_size] = 0
                self._totals[age_group][church][shirt_size] += 1
        else:
            # Update roster (all registrations go in roster)
            self._roster[church].append({"name": name, "shirt_size": shirt_size, "is_late": is_late})

            # Update totals - late registrations ONLY go in late_totals
            if is_late:
                if shirt_size not in self._late_totals[church]:
                    self._late_totals[church][shirt_size] = 0
                self._late_totals[church][shirt_size] += 1
            else:
                if shirt_size not in self._totals[church]:
                    self._totals[church][shirt_size] = 0
                self._totals[church][shirt_size] += 1

    def get_church_totals(self, church: str, age_group: Optional[str] = None, include_late: bool = False) -> dict:
        """
        Get shirt size totals for a specific church.

        Args:
            church: Church name
            age_group: Optional age group (for Camp events)
            include_late: If True, return only late registrations

        Returns:
            Dictionary of shirt sizes and counts
        """
        source = self._late_totals if include_late else self._totals

        if self.age_group_specific and age_group:
            return source[age_group].get(church, {})
        else:
            return source.get(church, {})

    def get_church_roster(self, church: str, age_group: Optional[str] = None) -> list:
        """
        Get the roster of names and shirt sizes for a specific church.

        Args:
            church: Church name
            age_group: Optional age group (for Camp events)

        Returns:
            List of dictionaries with name, shirt_size, and is_late
        """
        if self.age_group_specific and age_group:
            return self._roster[age_group].get(church, [])
        else:
            return self._roster.get(church, [])

    def get_all_churches(self, age_group: Optional[str] = None) -> list:
        """Get list of all churches with registrations."""
        if self.age_group_specific and age_group:
            return sorted(self._totals[age_group].keys())
        else:
            return sorted(self._totals.keys())

    def get_all_age_groups(self) -> list:
        """Get list of all age groups (only for age-specific events)."""
        if self.age_group_specific:
            return [AGE_GROUPS.HIGH_SCHOOL, AGE_GROUPS.MIDDLE_SCHOOL]
        return [None]

    def has_late_registrations(self, church: str, age_group: Optional[str] = None) -> bool:
        """Check if a church has any late registrations."""
        late_totals = self.get_church_totals(church, age_group, include_late=True)
        return len(late_totals) > 0

    @property
    def totals(self) -> dict:
        """Get all totals."""
        return self._totals

    @property
    def roster(self) -> dict:
        """Get all rosters."""
        return self._roster

    @property
    def late_totals(self) -> dict:
        """Get all late registration totals."""
        return self._late_totals
