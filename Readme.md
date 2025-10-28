# The Official Repository of "BABSA : A Large Scale Bangla Aspect Based Sentiment Analysis Dataset"
checking for changes

Additional Links: [For Our Main Dataset](./data/main/info.md) [For Our Selected Subsets](./data/subsets/info.md) 

Overview

BABSA is a large-scale, rigorously annotated dataset for Bangla Aspect-Based Sentiment Analysis (ABSA). Despite Bangla being one of the top ten most-spoken languages worldwide, ABSA resources for Bangla remain scarce. BABSA addresses this gap by combining existing datasets—Banglabook, SentNoB, EmoNoBa, and Sazzed—with a custom web-scraped dataset, providing high-quality annotations for both aspect extraction and sentiment classification.

We also benchmark BABSA using baseline and fine-tuned Large Language Models (LLMs) such as Gemma 3, Llama 3.1, Mistral, Orca 2, and Phi3, demonstrating improved performance in detecting aspect-specific sentiments in Bangla texts.


Features

1. High-quality annotations: Carefully annotated for aspect-level sentiment analysis.
2. Diverse sources: Combines multiple existing datasets and new web-scraped data.
3. Model benchmarks: Includes evaluation results for popular LLMs and fine-tuned models.
4. Open research resource: Designed to help researchers, students, and developers working on Bangla NLP tasks.


Dataset

The BABSA dataset includes:
1. Text data: Sentences and reviews in Bangla.
2. Aspect annotations: Identified aspects/topics in each sentence.
3. Sentiment labels: Positive, Negative, Neutral sentiment assigned per aspect.

Sources:
1. Banglabook
2. SentNoB
3. EmoNoBa
4. Sazzed
5. Custom web-scraped dataset

All data is preprocessed and formatted for easy use in ABSA experiments.


Installation / Usage

To use the dataset or run experiments:
```# Clone the repository
git clone https://github.com/your-username/BABSA.git

# Navigate to the repo
cd BABSA

# Install required dependencies (example)
pip install -r requirements.txt
```
You can then load the dataset in Python or other frameworks for ABSA tasks.
```
import pandas as pd

# Load the dataset
data = pd.read_csv("data/babsa_dataset.csv")
print(data.head())
```


Models & Benchmarking

We provide evaluation scripts for the following models:
1. Gemma 3
2. Llama 3.1
3. Mistral
4. Orca 2
5. Phi3

The evaluation scripts allow you to:
i. Fine-tune models on BABSA
ii. Benchmark performance
iii. Reproduce results reported in our paper


Citation

If you use BABSA in your research, please cite our paper:
```
@article{your2025babsa,
  title={BABSA: Bangla Aspect-Based Sentiment Analysis},
  author={Your Name et al.},
  journal={arXiv / Journal Name},
  year={2025}
}
```
Contributing
We welcome contributions!

Contact
For questions, reach out to: mfrforhad1@gmail.com
