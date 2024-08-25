import json
from collections import defaultdict

class MSAslScraper:
    def __init__(self, dataset_path):
        self.dataset_path = dataset_path
        self.data = None
        self.grouped_data = defaultdict(list)

    def load_data(self):
        try:
            with open(self.dataset_path, 'r') as f:
                self.data = json.load(f)
            print("Data loaded successfully.")
        except FileNotFoundError:
            print(f"Error: File not found at {self.dataset_path}")
        except json.JSONDecodeError:
            print("Error: Failed to decode JSON.")

    def group_by_org_text(self):
        if not self.data:
            print("No data to process. Make sure to load the data first.")
            return

        for entry in self.data:
            org_text = entry.get('org_text')
            if org_text:
                self.grouped_data[org_text].append(entry)

    def get_grouped_data(self):
        return dict(self.grouped_data)

    def save_grouped_data(self, output_path):
        try:
            with open(output_path, 'w') as f:
                json.dump(self.get_grouped_data(), f, indent=4)
            print(f"Grouped data saved to {output_path}")
        except Exception as e:
            print(f"Error saving grouped data: {e}")

# Example usage:
if __name__ == "__main__":
    scraper = MSAslScraper("../MSASL_train.json")
    scraper.load_data()
    scraper.group_by_org_text()
    grouped_data = scraper.get_grouped_data()

    # Optionally save the grouped data to a file
    scraper.save_grouped_data("grouped_ms_asl_dataset.json")
