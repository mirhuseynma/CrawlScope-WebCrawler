import React from "react";

type ModalNotificationProps = {
  message: string | null;
  type: "error" | "success";
  onClose: () => void;
};

export function ModalNotification({ message, type, onClose }: ModalNotificationProps) {
  if (!message) return null;

  return (
    <div style={{
      position: "fixed",
      top: 0, left: 0, right: 0, bottom: 0,
      backgroundColor: "rgba(0,0,0,0.5)",
      display: "flex",
      alignItems: "center",
      justifyContent: "center",
      zIndex: 9999
    }}>
      <div style={{
        backgroundColor: "#fff",
        padding: "24px",
        borderRadius: "8px",
        maxWidth: "400px",
        width: "100%",
        boxShadow: "0 4px 12px rgba(0,0,0,0.15)",
        textAlign: "center"
      }}>
        <h3 style={{ marginTop: 0, color: type === "error" ? "#e53e3e" : "#2d9a73" }}>
          {type === "error" ? "Error" : "Success"}
        </h3>
        <p style={{ color: "#4a5568", marginBottom: "20px" }}>{message}</p>
        <button className="primary-button" onClick={onClose} style={{ width: "100%" }}>
          Close
        </button>
      </div>
    </div>
  );
}
