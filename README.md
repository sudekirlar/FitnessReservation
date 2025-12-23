# Fitness Reservation System – Test Engineering Project

## Project Description

The Fitness Reservation System is a resilient, .NET-based backend application designed to manage fitness class reservations with **dynamic pricing**. The project was developed as part of a Software Test Engineering course and emphasizes advanced testing methodologies such as **Test-Driven Development (TDD)**, **Pairwise/Combinatorial Testing**, **Property-Based Testing**, **Performance Testing**, **Security Testing**, and **Chaos Engineering**.

The primary objective of the project is not only functional correctness, but also **reliability under stress, fault tolerance, and testability by design**.

---

## Key Features

* **Dynamic Pricing Engine**
  Pricing is calculated based on membership type, peak/off-peak time, and session occupancy levels.

* **Reservation Management**
  Prevents double booking, enforces session capacity limits, and validates temporal constraints (past vs. future sessions).

* **Resilient & Self-Healing Architecture**
  Fully Dockerized system validated through chaos experiments that simulate service failures.

* **Test-First Development**
  Core business logic is implemented using TDD with extensive unit, integration, and property-based tests.

---

## Technology Stack

| Tool / Framework | Version    | Purpose                        |
| ---------------- | ---------- | ------------------------------ |
| .NET SDK         | 8.0+ (LTS) | Backend framework              |
| ASP.NET Core     | 8.0        | Minimal API implementation     |
| SQLite           | 3.x        | Persistent database            |
| Docker           | 24.0+      | Containerization               |
| GitHub Actions   | Latest     | CI/CD pipeline                 |
| xUnit            | Latest     | Unit & integration testing     |
| FsCheck          | Latest     | Property-based testing         |
| PICT             | Latest     | Pairwise combinatorial testing |
| Postman          | 10+        | API test authoring             |
| Newman           | 6.0        | CLI execution of API tests     |
| k6               | 0.5x       | Performance & load testing     |
| OWASP ZAP        | 2.14+      | Automated security scanning    |
| Pumba            | Latest     | Chaos engineering              |

---

## How to Run the Project

### Prerequisites

* Docker & Docker Compose installed
* .NET SDK 8.0 (for local execution and tests)
* Node.js (for Newman)

### Clone the Repository

```bash
git clone https://github.com/sudekirlar/FitnessReservation.git
cd FitnessReservation
```

### Run with Docker

```bash
docker compose up --build
```

The API will be available at:

```
http://localhost:7001
```

Health check endpoint:

```bash
curl http://localhost:7001/health
```

---

## How to Execute Tests

### Unit & Integration Tests

Executes all TDD-based unit tests and integration tests:

```bash
dotnet test
```

---

### API Tests (Postman / Newman)

Runs end-to-end API scenarios including authentication, session listing, pricing, and reservations:

```bash
newman run postman/FitnessReservation.postman_collection.json \
  -e postman/FitnessReservation.ci.postman_environment.json
```

These tests are automatically executed in the CI pipeline.

---

### Performance Testing (k6)

Simulates user journeys and validates latency and error-rate thresholds:

```bash
k6 run k6/journey.js
```

Reported metrics include:

* Average response time
* P95 latency
* HTTP error rate

---

### Chaos Engineering (k6 + Pumba)

Chaos experiments validate system resilience under failure conditions.

Example experiment:

* Randomly terminate the API container while a k6 load test is running
* Observe recovery behavior and error rates

```bash
docker run --rm \
  -v /var/run/docker.sock:/var/run/docker.sock \
  gaiaadm/pumba kill --interval 15s --signal SIGKILL fitness-app
```

During chaos experiments:

* Temporary error rates are expected
* No data corruption occurs
* Service recovers automatically after restart

---

### Security Testing (OWASP ZAP)

Automated security scans are executed via CI using **OWASP ZAP Active Scan**:

* Detects common vulnerabilities (XSS, SQLi, misconfigurations)
* Fails the pipeline on newly introduced high-severity findings

ZAP reports are published as CI artifacts.

---

## Test Strategy Overview

* **Unit Tests:** Business rules (PricingEngine, ReservationService)
* **Integration Tests:** API ↔ Database interactions (in-memory SQLite)
* **Decision Tables:** Reservation and pricing rules derived into parameterized tests
* **Pairwise Testing:** Pricing parameters generated via PICT
* **Property-Based Tests:** Invariants such as capacity limits and monotonic pricing
* **Performance Tests:** k6 load and journey scenarios
* **Security Tests:** OWASP ZAP baseline scanning
* **Chaos Tests:** Pumba-based failure injection

This layered strategy ensures high confidence in both functional correctness and non-functional qualities.

---

## Continuous Integration

All tests are executed automatically using **GitHub Actions**:

1. Build
2. Unit & integration tests
3. Coverage report generation
4. API tests (Newman)
5. Security scan (ZAP)
6. Performance tests (k6)

Artifacts such as coverage reports, ZAP outputs, Newman reports, and k6 summaries are stored for each pipeline run.

---

## Conclusion

The Fitness Reservation System demonstrates the practical application of modern test engineering principles. By combining combinatorial testing, property-based testing, performance analysis, security scanning, and chaos engineering, the system achieves a high level of reliability and test coverage suitable for academic and real-world backend systems.
