# Spec Document

## 1. Overview

create a mvc page. it contains chian of responsibility.
Loan approval : If amount is less than 100 then noramal role user can approve
if amount more than 100 and less than 1000, then Supervisor role will approve
if amount is more than 1000 then manager will approve. 
Remmber: Loan will pass from Normal --> Supervisor --> Manager. Each user will check and delegate request to
next role

---

## 2. Depends on

Nothing — this is the first step.

---

## 3. Routes

Loan Approval
LoanApproverUser
LoanApproverSupervisor
LoanApproverManager


---

## 4. Database Schema

Create in c# object to hold infomration


## 5. Flow
User submit amount and basic info.
Normal Role will see details of submitted loans and use delegate process till approval
Once loan approved send email


