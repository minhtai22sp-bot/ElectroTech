using Entities.ViewModel;
using Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace ElectroTech.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly MailSettings _settings;

    public EmailService(IOptions<MailSettings> settings)
        => _settings = settings.Value;

    public async Task SendOrderConfirmationAsync(
        string toEmail,
        string toName,
        string orderCode,
        decimal total,
        List<(string Name, int Qty, decimal Price)> items,
        string shippingAddress)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = $"✅ Xác nhận đơn hàng #{orderCode} — ElectroTech";

        // Build HTML body
        var itemRows = string.Join("", items.Select(i => $"""
            <tr>
                <td style="padding:10px 16px;border-bottom:1px solid #f1f5f9;
                           font-size:14px;color:#374151;">{i.Name}</td>
                <td style="padding:10px 16px;border-bottom:1px solid #f1f5f9;
                           font-size:14px;color:#374151;text-align:center;">{i.Qty}</td>
                <td style="padding:10px 16px;border-bottom:1px solid #f1f5f9;
                           font-size:14px;color:#374151;text-align:right;">
                    {i.Price * i.Qty:N0}₫
                </td>
            </tr>
        """));

        var html = $"""
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset="UTF-8"/>
                <meta name="viewport" content="width=device-width,initial-scale=1.0"/>
            </head>
            <body style="margin:0;padding:0;background:#f8fafc;
                         font-family:'Segoe UI',Arial,sans-serif;">
                <div style="max-width:600px;margin:40px auto;background:#fff;
                            border-radius:16px;overflow:hidden;
                            box-shadow:0 4px 24px rgba(0,0,0,.08);">

                    <!-- Header -->
                    <div style="background:linear-gradient(135deg,#1e3a8a,#2563eb);
                                padding:32px 40px;text-align:center;">
                        <h1 style="margin:0;font-size:28px;font-weight:800;
                                   color:#fff;letter-spacing:-0.5px;">
                            ⚡ ElectroTech
                        </h1>
                        <p style="margin:8px 0 0;color:rgba(255,255,255,.8);
                                  font-size:14px;">
                            Cảm ơn bạn đã tin tưởng chúng tôi!
                        </p>
                    </div>

                    <!-- Body -->
                    <div style="padding:32px 40px;">

                        <div style="background:#f0fdf4;border:1px solid #86efac;
                                    border-radius:10px;padding:16px 20px;
                                    margin-bottom:24px;display:flex;
                                    align-items:center;gap:12px;">
                            <span style="font-size:24px;">✅</span>
                            <div>
                                <p style="margin:0;font-weight:700;color:#15803d;
                                          font-size:15px;">
                                    Đơn hàng đã được xác nhận!
                                </p>
                                <p style="margin:4px 0 0;color:#16a34a;font-size:13px;">
                                    Mã đơn hàng: <strong>#{orderCode}</strong>
                                </p>
                            </div>
                        </div>

                        <p style="margin:0 0 24px;font-size:15px;color:#374151;">
                            Xin chào <strong>{toName}</strong>,<br/>
                            Chúng tôi đã nhận được đơn hàng của bạn và đang tiến hành
                            xử lý. Dưới đây là thông tin chi tiết:
                        </p>

                        <!-- Order Items Table -->
                        <table style="width:100%;border-collapse:collapse;
                                      margin-bottom:20px;">
                            <thead>
                                <tr style="background:#f8fafc;">
                                    <th style="padding:10px 16px;text-align:left;
                                               font-size:12px;font-weight:700;
                                               color:#64748b;text-transform:uppercase;
                                               letter-spacing:.5px;
                                               border-bottom:2px solid #e2e8f0;">
                                        Sản phẩm
                                    </th>
                                    <th style="padding:10px 16px;text-align:center;
                                               font-size:12px;font-weight:700;
                                               color:#64748b;text-transform:uppercase;
                                               letter-spacing:.5px;
                                               border-bottom:2px solid #e2e8f0;">
                                        SL
                                    </th>
                                    <th style="padding:10px 16px;text-align:right;
                                               font-size:12px;font-weight:700;
                                               color:#64748b;text-transform:uppercase;
                                               letter-spacing:.5px;
                                               border-bottom:2px solid #e2e8f0;">
                                        Thành tiền
                                    </th>
                                </tr>
                            </thead>
                            <tbody>
                                {itemRows}
                            </tbody>
                            <tfoot>
                                <tr>
                                    <td colspan="2"
                                        style="padding:14px 16px;text-align:right;
                                               font-size:15px;font-weight:700;
                                               color:#0f172a;
                                               border-top:2px solid #e2e8f0;">
                                        Tổng cộng:
                                    </td>
                                    <td style="padding:14px 16px;text-align:right;
                                               font-size:18px;font-weight:800;
                                               color:#2563eb;
                                               border-top:2px solid #e2e8f0;">
                                        {total:N0}₫
                                    </td>
                                </tr>
                            </tfoot>
                        </table>

                        <!-- Shipping Info -->
                        <div style="background:#f8fafc;border-radius:10px;
                                    padding:16px 20px;margin-bottom:24px;">
                            <p style="margin:0 0 6px;font-size:12px;font-weight:700;
                                      color:#94a3b8;text-transform:uppercase;
                                      letter-spacing:.5px;">
                                Địa chỉ giao hàng
                            </p>
                            <p style="margin:0;font-size:14px;color:#374151;">
                                {shippingAddress}
                            </p>
                        </div>

                        <p style="margin:0;font-size:14px;color:#64748b;
                                  line-height:1.6;">
                            Chúng tôi sẽ thông báo cho bạn khi đơn hàng được giao.
                            Nếu có thắc mắc, vui lòng liên hệ với chúng tôi.
                        </p>
                    </div>

                    <!-- Footer -->
                    <div style="background:#f8fafc;padding:20px 40px;
                                text-align:center;
                                border-top:1px solid #e2e8f0;">
                        <p style="margin:0;font-size:12px;color:#94a3b8;">
                            © 2025 ElectroTech. Cảm ơn bạn đã mua hàng!
                        </p>
                    </div>
                </div>
            </body>
            </html>
        """;

        var bodyBuilder = new BodyBuilder { HtmlBody = html };
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(
            _settings.Host, _settings.Port,
            _settings.EnableSsl
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.None);

        await client.AuthenticateAsync(_settings.UserName, _settings.Password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}