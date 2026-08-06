# 🛒 Product Inventory Manager – C# Dictionary Application  
## PROG7312 Activity

GitHub Submission Link: [https://classroom.github.com/a/6J_G2A_A](https://classroom50.org/EMKNDN/emkndn-prog7312-g2-2026/assignments/prog7312-ice-task-3/accept)
---

## 📘 Scenario

You are tasked with building a simple **Product Inventory System** using the `Dictionary` data structure in C#.  
This system will manage products using their **ProductID** as the unique key and store product details like **Name** and **Price**.

> 📝 GUI implementation is your choice, but the core logic must be functional and testable.

---

## 🎯 Learning Goals

By completing this activity, you will:
- Understand how to use a **Dictionary** to store and manage key-value data
- Implement common operations: **Add**, **Update**, **Search**, **Delete**, and **Display**
- Practice **user interaction** through console prompts
- Enhance your app with a GUI for real-time display and interaction

---

## 📦 Instructions

### ✅ Step 1: Create the Product Dictionary

- Define a `Dictionary<int, Product>` where:
  - `int` is the ProductID
  - `Product` is a class or struct that contains `Name` and `Price`

- Add **at least 10 products** to your inventory. Example:

```csharp
ProductID = 101, Name = "Laptop", Price = R5000  
ProductID = 102, Name = "JBL Speaker", Price = R8000  
ProductID = 103, Name = "Tablet", Price = R2300  
// Add/Edit more depending on your preference
