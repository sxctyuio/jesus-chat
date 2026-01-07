< !doctype html >
 < html lang = "en" >
  < head >
  
    < meta charset = "utf-8" />
   
     < meta name = "viewport" content = "width=device-width,initial-scale=1" />
      
        < title > Local Proxy Chat</title>
           <style>
    :root
{ font - family: system - ui, -apple - system, Segoe UI, Roboto, Arial, sans-serif; }
body
{
margin: 0; background: #0b0b0b; color: #f2f2f2; }
    .wrap { max - width: 900px; margin: 0 auto; padding: 16px; display: grid; gap: 12px; }
    h1 { font - size: 18px; margin: 6px 0 0; font - weight: 650; }

    .card {
    background: #141414; border: 1px solid #262626; border-radius: 12px; padding: 12px; }
    label { display: block; font - size: 12px; opacity: 0.8; margin - bottom: 6px; }
        input, textarea, button {
        width: 100 %; box - sizing: border - box;
        background: #0f0f0f; color: #f2f2f2;
      border: 1px solid #2a2a2a; border-radius: 10px;
      padding: 10px 12px; font - size: 14px;
        }
        textarea { resize: vertical; min - height: 84px; }

    .row { display: grid; gap: 10px; grid - template - columns: 1fr 1fr; }
        @media(max - width: 700px) { .row { grid - template - columns: 1fr; } }

    .chat {
        height: 58vh; overflow: auto; padding: 10px;
        background:#0f0f0f; border-radius: 12px; border:1px solid #2a2a2a;
    }
    .msg { display: flex; margin: 10px 0; gap: 10px; }
    .role { width: 90px; font - size: 12px; opacity: 0.7; text - transform: uppercase; }
    .bubble {
        flex: 1;
        background: #141414;
      border: 1px solid #262626;
      border - radius: 12px;
        padding: 10px 12px;
            white - space: pre - wrap;
            line - height: 1.35;
        }
    .msg.user.bubble {
        background: #101a2a; border-color:#1f3558; }
    .msg.assistant.bubble {
            background: #141414; }
    .msg.system.bubble {
                background: #1a1210; border-color:#4a2b22; }
    .msg.error.bubble {
                    background:#2a1010; border-color:#6a2222; }

    .composer { display: flex; gap: 10px; }
    .composer textarea { min - height: 44px; height: 44px; max - height: 160px; }
                        button { cursor: pointer; font - weight: 650; }
                    button: disabled { opacity: 0.55; cursor: not - allowed; }
    .actions { display: flex; gap: 10px; flex - wrap: wrap; align - items:center; }
    .actions button { width: auto; }
    .tiny { font - size: 12px; opacity: 0.7; }
    .pill {
                        display: inline - block; padding: 4px 8px; border - radius: 999px;
                        border: 1px solid #2a2a2a; background:#0f0f0f;
      font - size: 12px; opacity: 0.85;
                        }
  </ style >
</ head >

< body >
  < div class= "wrap" >
 
     < h1 > Chat(via Local Proxy) </ h1 >
 

     < div class= "card" >
  
        < div class= "row" >
   
           < div >
   
             < label > Proxy URL </ label >
      
                < input id = "proxyUrl" value = "http://localhost:3000/chat" />
         
                   < div class= "tiny" style = "margin-top:6px;" >
                       Your Node proxy must be running in Terminal: < span class= "pill" > node server.js </ span >
               
                         </ div >
               
                       </ div >
               
                       < div >
               
                         < label > Model </ label >
               
                         < input id = "model" value = "gpt-3.5-turbo" />
                  
                            < div class= "tiny" style = "margin-top:6px;" >
                                You can change this if your proxy allows it.
          </div>
        </div>
      </div>

      <div style="margin-top:10px;">
        <label>System Prompt</label>
        <textarea id="systemPrompt">You are a helpful assistant. Keep responses concise.</textarea>
      </div>

      <div class= "actions" style = "margin-top:10px;" >
 
         < button id = "newChatBtn" > New chat </ button >
      
              < button id = "saveBtn" > Save </ button >
       
               < button id = "loadBtn" > Load </ button >
        
                < span class= "tiny" id = "status" ></ span >
          
                </ div >
          
              </ div >
          

              < div class= "chat" id = "chat" ></ div >
            

                < div class= "card" >
             
                   < div class= "composer" >
              
                      < textarea id = "userInput" placeholder = "Type a message... (Enter to send, Shift+Enter for new line)" ></ textarea >
                 
                         < button id = "sendBtn" > Send </ button >
                  
                        </ div >
                  
                        < div class= "tiny" style = "margin-top:8px;" >
                            Tip: If you see a network error, confirm your proxy is running and the URL is correct.
      </div>
    </div>
  </div>

<script>
  // UI refs
  const chatEl = document.getElementById("chat");
const proxyUrlEl = document.getElementById("proxyUrl");
const modelEl = document.getElementById("model");
const systemPromptEl = document.getElementById("systemPrompt");
const userInputEl = document.getElementById("userInput");
const sendBtn = document.getElementById("sendBtn");
const statusEl = document.getElementById("status");

// Conversation state (we keep system separately; only user/assistant here)
let messages = [];

function setStatus(text)
{
    statusEl.textContent = text || "";
}

function render()
{
    chatEl.innerHTML = "";

    // Show system prompt as the first visible message (UI-only)
    const sysText = systemPromptEl.value.trim() || "(no system prompt)";
    chatEl.appendChild(makeRow("system", sysText));

    for (const m of messages) {
    chatEl.appendChild(makeRow(m.role, m.content));
}
chatEl.scrollTop = chatEl.scrollHeight;
  }

  function makeRow(role, content)
{
    const row = document.createElement("div");
    row.className = "msg " + role;

    const roleEl = document.createElement("div");
    roleEl.className = "role";
    roleEl.textContent = role;

    const bubble = document.createElement("div");
    bubble.className = "bubble";
    bubble.textContent = content;

    row.appendChild(roleEl);
    row.appendChild(bubble);
    return row;
}

function resetChat()
{
    messages = [];
    render();
    setStatus("");
    userInputEl.focus();
}

// Local save/load (no API keys stored)
function saveToBrowser()
{
    const payload = {
      proxyUrl: proxyUrlEl.value,
      model: modelEl.value,
      systemPrompt: systemPromptEl.value,
      messages
    };
localStorage.setItem("local_proxy_chat", JSON.stringify(payload));
setStatus("Saved.");
setTimeout(() => setStatus(""), 1200);
  }

  function loadFromBrowser()
{
    const raw = localStorage.getItem("local_proxy_chat");
    if (!raw) { setStatus("Nothing saved yet."); return; }
    try
    {
        const payload = JSON.parse(raw);
        if (payload.proxyUrl) proxyUrlEl.value = payload.proxyUrl;
        if (payload.model) modelEl.value = payload.model;
        if (payload.systemPrompt != null) systemPromptEl.value = payload.systemPrompt;
        if (Array.isArray(payload.messages)) messages = payload.messages;
        render();
        setStatus("Loaded.");
        setTimeout(() => setStatus(""), 1200);
    }
    catch (e)
    {
        setStatus("Load failed (bad data).");
    }
}

function buildRequestBody(userText)
{
    const systemPrompt = systemPromptEl.value.trim();
    const model = modelEl.value.trim() || "gpt-3.5-turbo";

    // Send full chat history (user+assistant), plus system
    const reqMessages = [];
    if (systemPrompt) reqMessages.push({ role: "system", content: systemPrompt });
reqMessages.push(...messages);
reqMessages.push({ role: "user", content: userText });

return {
    model,
      messages: reqMessages,
      temperature: 0.7
    };
  }

  async function callProxy(userText)
{
    const proxyUrl = proxyUrlEl.value.trim();
    if (!proxyUrl) throw new Error("Missing proxy URL.");

    const res = await fetch(proxyUrl, {
    method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(buildRequestBody(userText))
    });

// Proxy returns OpenAI-style JSON
const text = await res.text();
let data;
try { data = JSON.parse(text); } catch { /* keep text */ }

if (!res.ok)
{
    const msg = data?.error?.message || text || `HTTP ${ res.status}`;
    throw new Error(msg);
}

const reply = data?.choices?.[0]?.message?.content;
if (!reply) throw new Error("No reply returned.");
return reply;
  }

  async function onSend()
{
    const text = userInputEl.value.trim();
    if (!text) return;

    // Optimistically add user message
    messages.push({ role: "user", content: text });
userInputEl.value = "";
render();

sendBtn.disabled = true;
userInputEl.disabled = true;
setStatus("Thinking...");

try
{
    const reply = await callProxy(text);
    messages.push({ role: "assistant", content: reply });
    render();
    setStatus("");
}
catch (err)
{
    messages.push({ role: "error", content: "Error: " + (err?.message || String(err)) });
    render();
    setStatus("");
}
finally
{
    sendBtn.disabled = false;
    userInputEl.disabled = false;
    userInputEl.focus();
}
  }

  // Events
  document.getElementById("newChatBtn").addEventListener("click", resetChat);
document.getElementById("saveBtn").addEventListener("click", saveToBrowser);
document.getElementById("loadBtn").addEventListener("click", loadFromBrowser);
sendBtn.addEventListener("click", onSend);

userInputEl.addEventListener("keydown", (e) => {
    if (e.key === "Enter" && !e.shiftKey)
    {
        e.preventDefault();
        onSend();
    }
});

// Init
resetChat();
</ script >
</ body >
</ html >
