# prior-auth-portal
A full-stack prior authorization portal modeling the B2B workflow between prescriber offices and payers, built with .NET Minimal API, React, Azure, and FHIR R5.

Three clinical scenarios are automatically routed to manual review rather than auto-adjudicated:

1) Genetic testing for Hereditary Breast/Ovarian Cancer (BRCA1/BRCA2)
2) Wegovy
3) Humira for Rheumatoid Arthritis

These scenarios reflect real-world PA workflows where high-cost biologics, weight loss medications, and complex genetic indications typically require clinical reviewer sign-off regardless of structured criteria.