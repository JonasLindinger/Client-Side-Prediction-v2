# 🚀 Client-Side Prediction v2

> AI GENERATED README based on a long text of detailed instructions written by me.

A **modular**, **easy-to-use**, and **high-performance** Client-Side Prediction (CSP) system built for **Unity’s Netcode for GameObjects**.

Supports:
- ✅ **Direct position access**
- ✅ **Rigidbody-based movement** (used in the demo)
- ⚙️ **Character Controllers** *(not tested, but should work)*

---

> 💡 **Info:**  
> Copy the `CSP`, `SceneManagement` (optional), and `Singletons` (required) folders into your own project.  
> Then, check out the demo under `_Project` to understand how the system works.

---

## 🧠 About This Project

This system implements **Client-Side Prediction Level 4**, meaning:

- The **client** sends inputs to the **server** while also **predicting its own movement**.
- The **server** processes inputs at a fixed rate — spamming won’t give unfair advantages.
- The **server** sends back the authoritative position.
- The **client** checks prediction accuracy and, if necessary, **reconciles past states**.
- Everything runs on **synced ticks** between all clients and the server, allowing features like:
  - 🏐 Networked physics objects (e.g. a Rocket League-style ball)
  - ⏪ Collider rollback
  - ⏱️ Tick-based time stamping

### 🧩 Technical Highlights

- **Upstream Throttling** keeps tick systems in sync by skipping or adding extra ticks when needed.  
- **Unreliable RPCs** for fast communication (with input buffering to handle packet loss).  
- **Circular Buffers** store the last `1024` game states and inputs (configurable via ScriptableObject).  
- **Configurable input batching**: the client can send the current input plus the last 7 inputs to fill server buffers.  

---

## 📘 Development Story

> “When I started this project, I didn’t even use GitHub.”

This system is the result of **two years** of research, iteration, and passion for networking.

I studied **GDC Vault talks**, **YouTube resources**, and a lot of **theoretical networking principles** to build a fully working, **secure** and **complete** Client-Side Prediction system for Unity.

There’s no real coding tutorial out there for **Unity Netcode for GameObjects CSP**,  
so I decided to make one — **from scratch**.

I even wrote **33 pages** on the different “Levels” of Client-Side Prediction as a school project (in **German** 🇩🇪).  
That document earned me a top grade and gave me a deep understanding of how to design **accurate**, **time-consistent**, and **modular** network systems.

If you’re interested in those 33 pages, feel free to open an **Issue** or contact me —  
but keep in mind, it’s in German (you might need a translator 😄).

---

## 🔮 Future Plans

- 🧱 **Level 5 Support** — Predicting and reconciling other networked objects (e.g., physics balls)  
- 🧹 **Cleaner Codebase** — Refactoring old systems for readability and maintainability  
- 🎥 **Tutorial Video** — Once the system is stable enough, I may release a coding breakdown  

---

## 🤝 Contribution & Support

If you run into issues or have questions:
- Open an **Issue** here on GitHub
- Or contact me directly

I’ll do my best to help you out. 💬

---

## 🧾 Credits

Inspired by:
- GDC Vault networking talks  
- YouTube creators explaining CSP theory  
- Countless hours of experimentation and debugging 😅  

---

## 💡 Summary

| Feature | Description |
|----------|--------------|
| 🎮 Prediction Level | **Level 4** (fully predicted client) |
| 🔁 Tick Sync | Global tick system for all clients and server |
| 🧱 Input Buffer | Last 1024 states & inputs (configurable) |
| ⚡ Network Transport | Unity Netcode for GameObjects |
| 🧩 Modularity | Easy to integrate and extend |
| 🔒 Security | Input spam prevention & tick-based validation |

---

