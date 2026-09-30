import { useState } from "react";
import { he } from "../strings.he";
import { Icon } from "./Icon";
import { Ltr, Modal } from "./ui";

export function Footer({ updatedDate }: { updatedDate: string | null }) {
  const [privacyOpen, setPrivacyOpen] = useState(false);
  return (
    <footer className="footer">
      <div className="container footer__inner">
        <p className="footer__source">
          <Icon name="document" size={16} />
          {he.footer.source}
        </p>
        {updatedDate && (
          <p className="footer__updated">
            {he.footer.updated("")}
            <Ltr>{updatedDate}</Ltr>
          </p>
        )}
        <p className="footer__disclaimer">{he.footer.disclaimer}</p>
        <button type="button" className="link-btn footer__privacy no-print" onClick={() => setPrivacyOpen(true)}>
          {he.footer.privacy}
        </button>
      </div>
      <Modal open={privacyOpen} onClose={() => setPrivacyOpen(false)} title={he.footer.privacy} icon="shield">
        <p>{he.footer.privacyText}</p>
      </Modal>
    </footer>
  );
}
